using System;
using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Serializer
{
    public class SerializerResolver
    {
        private readonly Dictionary<Type, ISerializer> serializers = new Dictionary<Type, ISerializer>();

        public bool IsTypeRegistered(Type type) => serializers.ContainsKey(type);

        public bool CanResolve(Type type, int length)
        {
            if (!IsTypeRegistered(type))
            {
                return false;
            }

            var serializer = serializers[type];
            while (serializer != null)
            {
                if (serializer.IsLengthValid(length))
                {
                    return true;
                }

                serializer = serializer.Next;
            }

            return false;
        }

        public void AddSerializer(ISerializer serializer)
        {
            if (serializers.ContainsKey(serializer.Type))
            {
                serializer.Next = serializers[serializer.Type];
            }

            serializers[serializer.Type] = serializer;
        }

        public void AddBasicSerializers()
        {
            AddSerializer(new BoolSerializer());
            AddSerializer(new ByteSerializer());
            AddSerializer(new DateTimeSerializer());
            AddSerializer(new IntSerializer());
            AddSerializer(new LongSerializer());
            AddSerializer(new SByteSerializer());
            AddSerializer(new ShortSerializer());
            AddSerializer(new StringSerializer());
            AddSerializer(new TimeSpanSerializer());
            AddSerializer(new UIntSerializer());
            AddSerializer(new ULongSerializer());
            AddSerializer(new UShortSerializer());
        }

        public ISerializer ResolveSerializer(Type type)
        {
            if (serializers.ContainsKey(type))
            { 
                return serializers[type];
            }

            return null;
        }

        public ISerializer ResolveSerializer<T>() => ResolveSerializer(typeof(T));
    }
}
