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
    public class Serializer
    {
        private ModelDefinitionParser _parser;
        private SerializerResolver _resolver;

        public Serializer(ModelDefinitionParser parser, SerializerResolver resolver)
        { 
            _parser = parser;
            _resolver = resolver;
        }

        public int CalculateLength(Type type, object obj)
        {
            var len = 0;
            var definitions = _parser.GetDefinition(type);
            var properties = type.GetProperties();
            foreach (var definition in definitions)
            {
                var property = properties.Single(x => x.Name == definition.Name);
                var propertyInstance = (dynamic)property.GetValue(obj);
                if (definition.IsEnumerable)
                {
                    len += sizeof(int);
                    var count = Enumerable.Count(propertyInstance);
                    
                    if (definition.Length == -1)
                    {
                        len += sizeof(int) * count;
                    }

                    var serializer = _resolver.ResolveSerializer(property.PropertyType.GetGenericArguments()[0]);
                    foreach (var item in propertyInstance)
                    {
                        len += serializer.CalculateLength(item, property.GetCustomAttributes(true).Select(x => x as Attribute));
                    }
                }
                else
                {
                    var serializer = _resolver.ResolveSerializer(property.PropertyType);
                    if (definition.Length == -1)
                    {
                        len += sizeof(int);
                    }
                    len += serializer.CalculateLength(propertyInstance, property.GetCustomAttributes(true).Select(x => x as Attribute));
                }
            }

            return len;
        }

        public int Serialize(object obj, Span<byte> destination)
        {
            var intSerializer = _resolver.ResolveSerializer(typeof(int));
            var len = 0;
            var definitions = _parser.GetDefinition(obj);
            var type = obj.GetType();
            var properties = type.GetProperties();
            foreach (var definition in definitions)
            {
                var property = properties.Single(x => x.Name == definition.Name);
                var propertyInstance = (dynamic)property.GetValue(obj);
                var attributes = property.GetCustomAttributes(true).Select(x => x as Attribute);
                if (definition.IsEnumerable)
                {
                    int count = Enumerable.Count(propertyInstance);
                    intSerializer.WriteBytes(count, destination.Slice(len), attributes);
                    len += sizeof(int);
                    var serializer = _resolver.ResolveSerializer(property.PropertyType.GetGenericArguments()[0]);
                    foreach (var item in propertyInstance)
                    {
                        int itemLength = serializer.CalculateLength(item, attributes);
                        if (definition.Length == -1)
                        {
                            intSerializer.WriteBytes(itemLength, destination.Slice(len), attributes);
                            len += sizeof(int);
                        }
                        serializer.WriteBytes((object)item, destination.Slice(len), attributes);
                        len += itemLength;
                    }
                }
                else
                {
                    var serializer = _resolver.ResolveSerializer(property.PropertyType);
                    int itemLength = serializer.CalculateLength(propertyInstance, attributes);
                    if (definition.Length == -1)
                    {
                        intSerializer.WriteBytes(itemLength, destination.Slice(len), attributes);
                        len += sizeof(int);
                    }
                    serializer.WriteBytes((object)propertyInstance, destination.Slice(len), attributes);
                    len += itemLength;
                }
            }

            return len;
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

        public static string ComputeModelDefinitionHash(IEnumerable<ModelDefinition> definitions)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(definitions));
            var hash = Sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static Type GetOrCreateTypeFromDefinition(IEnumerable<ModelDefinition> definitions)
        {
            var hash = ComputeModelDefinitionHash(definitions);

            if (!dynamicTypeCache.ContainsKey(hash))
            {
                var type = container.CreateClass("dynamic_" + Guid.NewGuid().ToString().Replace("-", ""));

                foreach (var def in definitions)
                {
                    var t = Type.GetType(def.ClrType);
                    type.AddProperty(t, def.Name, ActivateAttributes(def.Attributes));
                }

                dynamicTypeCache[hash] = type.Build();
            }

            return dynamicTypeCache[hash];
        }
    }
}
