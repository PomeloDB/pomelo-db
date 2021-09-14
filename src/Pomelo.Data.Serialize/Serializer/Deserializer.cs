using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Pomelo.Data.Serialize.Definition;
using Pomelo.Data.Serialize.Dynamic;
using Newtonsoft.Json;

namespace Pomelo.Data.Serialize.Serializer
{
    public class Deserializer
    {
        private ModelDefinitionParser _parser;
        private SerializerResolver _resolver;

        public Deserializer(ModelDefinitionParser parser, SerializerResolver resolver)
        { 
            _parser = parser;
            _resolver = resolver;
        }

        private static string dynamicAssemblyName = Guid.NewGuid().ToString().Replace("-", "");

        private static ClassContainer container = new ClassContainer(dynamicAssemblyName);

        public static IEnumerable<Attribute> ActivateAttributes(IDictionary<string, IDictionary<string, object>> attributeDefinitions)
        {
            foreach (var def in attributeDefinitions)
            { 
                var type = Type.GetType(def.Key);
                var attr = Activator.CreateInstance(type);
                foreach (var property in def.Value)
                {
                    type.GetProperty(property.Key).SetValue(attr, property.Value);
                }
                yield return (Attribute)attr;
            }
        }

        private static SHA256 Sha256 = SHA256.Create();

        private static Dictionary<string, Type> dynamicTypeCache = new Dictionary<string, Type>();

        public static string ComputeModelDefinitionHash(IEnumerable<ModelPropertyDefinition> definitions)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(definitions));
            var hash = Sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static Type GetOrCreateTypeFromDefinition(IEnumerable<ModelPropertyDefinition> definitions)
        {
            var hash = ComputeModelDefinitionHash(definitions);

            if (!dynamicTypeCache.ContainsKey(hash))
            {
                var type = container.CreateClass("dynamic_" + Guid.NewGuid().ToString().Replace("-", ""));

                foreach (var def in definitions)
                {
                    var t = Type.GetType(def.ClrType);
                    if (def.IsEnumerable)
                    {
                        var t2 = typeof(List<>);
                        var t3 = t2.MakeGenericType(t);
                        type.AddProperty(t3, def.Name, ActivateAttributes(def.Attributes));
                    }
                    else
                    {
                        type.AddProperty(t, def.Name, ActivateAttributes(def.Attributes));
                    }
                }

                dynamicTypeCache[hash] = type.Build();
            }

            return dynamicTypeCache[hash];

        }

        public object Deserialize(Span<byte> source, ModelDefinition definition)
        {
            var type = GetOrCreateTypeFromDefinition(definition.Properties);
            return Deserialize(type, source);
        }

        public object Deserialize(Type type, Span<byte> source)
        { 
            var obj = Activator.CreateInstance(type);
            var properties = type.GetProperties();
            var def = _parser.GetDefinition(type);
            var intSerializer = _resolver.ResolveSerializer(typeof(int));
            var len = 0;
            foreach (var propertyDef in def.Properties)
            {
                var property = properties.Single(x => x.Name == propertyDef.Name);
                var attributes = property.GetCustomAttributes(true).Select(x => x as Attribute);
                if (propertyDef.IsEnumerable)
                {
                    var propertyValue = Activator.CreateInstance(property.PropertyType);
                    property.SetValue(obj, propertyValue);
                    var count = (int)intSerializer.GetValue(source.Slice(len, intSerializer.DefaultLength));
                    len += intSerializer.DefaultLength;
                    var serializer = _resolver.ResolveSerializer(property.PropertyType.GetGenericArguments()[0]);
                    for (var i = 0; i < count; ++i)
                    {
                        if (serializer.IsFlexLength)
                        {
                            var length = (int)intSerializer.GetValue(source.Slice(len, intSerializer.DefaultLength));
                            len += intSerializer.DefaultLength;
                            var value = serializer.GetValue(source.Slice(len, length), attributes);
                            len += length;
                            property.PropertyType.GetMethod("Add").Invoke(propertyValue, new object[] { value });
                        }
                        else
                        {
                            var value = serializer.GetValue(source.Slice(len, serializer.DefaultLength), attributes);
                            len += serializer.DefaultLength;
                            property.PropertyType.GetMethod("Add").Invoke(propertyValue, new object[] { value });
                        }
                    }
                }
                else
                {
                    var serializer = _resolver.ResolveSerializer(property.PropertyType);
                    if (serializer.IsFlexLength)
                    {
                        var length = (int)intSerializer.GetValue(source.Slice(len, intSerializer.DefaultLength));
                        len += intSerializer.DefaultLength;
                        var value = serializer.GetValue(source.Slice(len, length), attributes);
                        len += length;
                        property.SetValue(obj, value);
                    }
                    else
                    {
                        var value = serializer.GetValue(source.Slice(len, serializer.DefaultLength), attributes);
                        len += serializer.DefaultLength;
                        property.SetValue(obj, value);
                    }
                }
            }
            return obj;
        }
    }
}
