using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Pomelo.Data.Serialize.Serializer;

namespace Pomelo.Data.Serialize.Definition
{
    public class ModelDefinitionParser
    {
        private SerializerResolver _resolver;
        private Dictionary<Type, List<ModelDefinition>> _cache;

        public ModelDefinitionParser(SerializerResolver resolver)
        {
            _resolver = resolver;
            _cache = new Dictionary<Type, List<ModelDefinition>>();
        }

        public virtual IEnumerable<ModelDefinition> GetDefinition(object obj)
        {
            var type = obj.GetType();
            if (_cache.ContainsKey(type))
            { 
                return _cache[type];
            }

            var properties = type.GetProperties();
            var ret = new List<ModelDefinition>(properties.Length);
            foreach (var property in properties)
            {
                var definition = GetPropertyDefinition(property);
                if (definition != null)
                {
                    ret.Add(definition);
                }
            }

            _cache[type] = ret.OrderBy(x => x.Order).ToList();
            return _cache[type];
        }

        protected virtual ModelDefinition GetPropertyDefinition(PropertyInfo property)
        {
            if (property.GetCustomAttribute<IgnoreAttribute>() != null)
            {
                return null;
            }

            var definition = new ModelDefinition();

            // Parsing Name
            definition.Name = property.Name;

            // Parsing Clr Type
            var propertyType = property.PropertyType;
            var enumerable = GetEnumerableRecursive(property.PropertyType);
            if (enumerable != null && property.PropertyType != typeof(string))
            {
                definition.IsEnumerable = true;
                propertyType = enumerable.GenericTypeArguments[0];
            }
            if (propertyType.IsEnum)
            {
                propertyType = propertyType.GetEnumUnderlyingType();
            }
            definition.ClrType = propertyType.Name;

            if (!_resolver.IsTypeRegistered(propertyType))
            {
                return null;
            }

            // Parsing Length
            definition.Length = -1;
            var fixedLengthAttribute = property.GetCustomAttribute<FixedLengthAttribute>();
            if (fixedLengthAttribute != null)
            {
                definition.Length = fixedLengthAttribute.Length;
            }
            else if (propertyType.IsValueType && TryGetTypeSize(propertyType, out var length))
            {
                definition.Length = length;
            }

            // Check Length
            if (!_resolver.CanResolve(propertyType, definition.Length))
            {
                return null;
            }

            // Parsing Order
            var orderAttribute = property.GetCustomAttribute<OrderAttribute>();
            if (orderAttribute != null)
            {
                definition.Order = orderAttribute.Order;
            }
            else
            {
                definition.Order = int.MaxValue;
            }

            definition.Attributes = ParseAttributesInfo(property.GetCustomAttributes());

            return definition;
        }

        protected virtual IDictionary<string, IDictionary<string, object>> ParseAttributesInfo(IEnumerable<Attribute> attributes)
        {
            var ret = new Dictionary<string, IDictionary<string, object>>();

            attributes = attributes.Where(x => x.GetType().GetInterfaces().Contains(typeof(ISerializeOptionAttribute)));
            foreach (var attribute in attributes)
            {
                ret.Add(attribute.GetType().Name, ParseAttributeInfo(attribute));
            }

            return ret;
        }

        protected virtual IDictionary<string, object> ParseAttributeInfo(Attribute attribute)
        {
            var ret = new Dictionary<string, object>();

            var properties = attribute.GetType().GetProperties(BindingFlags.Public);
            foreach (var property in properties)
            {
                ret.Add(property.Name, property.GetValue(attribute));
            }

            return ret;
        }

        protected virtual bool TryGetTypeSize(Type type, out int size)
        {
            size = -1;
            try
            {
                size = Marshal.SizeOf(type);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private Type GetEnumerableRecursive(Type type)
        {
            foreach (var t in GetAllBaseTypes(type))
            { 
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return t;
                }
            }

            return null;
        }

        private IEnumerable<Type> GetAllBaseTypes(Type type)
        {
            var baseType = type.BaseType;

            if (baseType != null)
            {
                yield return baseType;

                foreach (var t in GetAllBaseTypes(baseType))
                {
                    yield return t;
                }
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                yield return interfaceType;

                foreach (var t in GetAllBaseTypes(interfaceType))
                {
                    yield return t;
                }
            }
        }
    }
}
