using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Pomelo.Data.Serialize.Dynamic
{
    public class ClassBuilder
    {
        public const string GeneratedNamespace = "Pomelo.Data.Serialize.DynamicProxy.Generated";
        ClassContainer _proxyBuilder;
        ModuleBuilder _moduleBuilder;
        TypeBuilder _typeBuilder;
        string _namespace;

        public ClassBuilder(ClassContainer proxyBuilder, string className, string @namespace = null)
        {
            _namespace = @namespace ?? GeneratedNamespace;
            _proxyBuilder = proxyBuilder;
            _moduleBuilder = _proxyBuilder.AssemblyBuilder.DefineDynamicModule(_namespace);
            _typeBuilder = _moduleBuilder.DefineType(className);
        }

        public void AddProperty<T>(
            string name,
            IEnumerable<Attribute> attributes = null,
            Func<object, object> getter = null,
            MethodAttributes getterAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            Action<object, object> setter = null,
            MethodAttributes setterAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig)
        {
            AddProperty(typeof(T), name, attributes, getter, getterAttributes, setter, setterAttributes);
        }

        public void AddProperty(
            Type type,
            string name, 
            IEnumerable<Attribute> attributes = null,
            Func<object, object> getter = null, 
            MethodAttributes getterAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, 
            Action<object, object> setter = null, 
            MethodAttributes setterAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig)
        {
            var fieldBuilder = _typeBuilder.DefineField("_" + name, type, FieldAttributes.Private);
            var getterBuilder = _typeBuilder.DefineMethod("get_" + name, getterAttributes, type, null);
            var setterBuilder = _typeBuilder.DefineMethod("set_" + name, setterAttributes, null, new Type[] { type });

            var getterIlGenerator = getterBuilder.GetILGenerator();
            if (getter == null)
            {
                getterIlGenerator.Emit(OpCodes.Ldarg_0);
                getterIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
                getterIlGenerator.Emit(OpCodes.Ret);
            }
            else
            {
                var getterName = _namespace + ".get_" + name;
                var getterLambdaType = typeof(Func<object, object>);
                LambdaRegistery.RegisterGetter(getterName, getter);
                var registryType = typeof(LambdaRegistery);
                getterIlGenerator.Emit(OpCodes.Ldstr, getterName);
                getterIlGenerator.Emit(OpCodes.Call, registryType.GetMethod("FindGetter"));
                getterIlGenerator.Emit(OpCodes.Ldarg_0);
                getterIlGenerator.Emit(OpCodes.Call, getterLambdaType.GetMethod("Invoke"));
                getterIlGenerator.Emit(OpCodes.Call, registryType.GetMethod("Cast").MakeGenericMethod(new Type[] { type }));
                getterIlGenerator.Emit(OpCodes.Ret);
            }

            var setterIlGenerator = setterBuilder.GetILGenerator();
            if (setter == null)
            {
                setterIlGenerator.Emit(OpCodes.Ldarg_0);
                setterIlGenerator.Emit(OpCodes.Ldarg_1);
                setterIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);
                setterIlGenerator.Emit(OpCodes.Ret);
            }
            else
            {
                var setterName = _namespace + ".set_" + name;
                var setterLambdaType = typeof(Action<object, object>);
                LambdaRegistery.RegisterSetter(setterName, setter);
                var registryType = typeof(LambdaRegistery);
                setterIlGenerator.Emit(OpCodes.Ldstr, setterName);
                setterIlGenerator.Emit(OpCodes.Call, registryType.GetMethod("FindSetter"));
                setterIlGenerator.Emit(OpCodes.Ldarg_0);
                setterIlGenerator.Emit(OpCodes.Ldarg_1);
                if (type.IsValueType)
                {
                    setterIlGenerator.Emit(OpCodes.Box, type);
                }
                setterIlGenerator.Emit(OpCodes.Call, setterLambdaType.GetMethod("Invoke"));
                setterIlGenerator.Emit(OpCodes.Ret);
            }

            var propertyBuilder = _typeBuilder.DefineProperty(name, PropertyAttributes.None, type, null);
            propertyBuilder.SetGetMethod(getterBuilder);
            propertyBuilder.SetSetMethod(setterBuilder);
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    var ctor = attribute
                        .GetType()
                        .GetConstructors()
                        .Where(x => x.IsPublic && !x.IsStatic)
                        .Where(x => x.GetParameters().Count() == 0)
                        .First();

                    var attributeInfo = ParseAttribute(attribute);
                    propertyBuilder.SetCustomAttribute(new CustomAttributeBuilder(ctor, null, attributeInfo.properties, attributeInfo.values));
                }
            }
        }

        public Type Build()
        {
            return _typeBuilder.CreateType();
        }

        private (PropertyInfo[] properties, object[] values) ParseAttribute(Attribute attribute)
        {
            var properties = attribute
                .GetType()
                .GetProperties()
                .Where(x => x.CanWrite && x.CanRead);

            var len = properties.Count();
            var retProperties = new PropertyInfo[len];
            var retValues = new object[len];

            var i = 0;
            foreach (var property in properties)
            {
                retProperties[i] = property;
                retValues[i] = property.GetGetMethod().Invoke(attribute, null);
                ++i;
            }

            return (retProperties, retValues);
        }
    }
}
