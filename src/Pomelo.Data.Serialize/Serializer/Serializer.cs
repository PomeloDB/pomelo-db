using System;
using System.Linq;
using Pomelo.Data.Serialize.Definition;

namespace Pomelo.Data.Serialize.Serializer
{
    public class Serializer<T>
    {
        private ModelDefinitionParser _parser;
        private SerializerResolver _resolver;

        public Serializer(ModelDefinitionParser parser, SerializerResolver resolver)
        { 
            _parser = parser;
            _resolver = resolver;
        }

        public int CalculateLength(T obj)
        {
            var len = 0;
            var definitions = _parser.GetDefinition(obj);
            var type = typeof(T);
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

        public int Serialize(T obj, Span<byte> destination)
        {
            var intSerializer = _resolver.ResolveSerializer(typeof(int));
            var len = 0;
            var definitions = _parser.GetDefinition(obj);
            var type = typeof(T);
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
    }
}
