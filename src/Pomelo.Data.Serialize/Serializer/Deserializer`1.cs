using System;
using Pomelo.Data.Serialize.Definition;

namespace Pomelo.Data.Serialize.Serializer
{
    public class Deserializer<T> : Deserializer
    {
        public Deserializer(ModelDefinitionParser parser, SerializerResolver resolver) : base(parser, resolver)
        { 
        }

        public object Deserialize(Span<byte> source)
        {
            return Deserialize(typeof(T), source);
        }
    }
}
