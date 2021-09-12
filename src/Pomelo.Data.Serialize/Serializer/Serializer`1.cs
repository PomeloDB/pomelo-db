using System;
using Pomelo.Data.Serialize.Definition;

namespace Pomelo.Data.Serialize.Serializer
{
    public class Serializer<T> : Serializer
    {
        public Serializer(ModelDefinitionParser parser, SerializerResolver resolver) : base(parser, resolver)
        { 
        }

        public int CalculateLength(T obj)
        {
            return CalculateLength(typeof(T), obj);
        }

        public int Serialize(T obj, Span<byte> destination)
        {
            return Serialize((object)obj, destination);
        }
    }
}
