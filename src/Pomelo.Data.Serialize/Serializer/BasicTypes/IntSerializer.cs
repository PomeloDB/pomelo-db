using System;
using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Serializer
{
    public class IntSerializer : SerializerBase<int>
    {
        public override int GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null)
        {
            return BitConverter.ToInt32(value);
        }

        public override void WriteBytes(int value, Span<byte> destination, IEnumerable<Attribute> attributes = null)
        {
            if (!BitConverter.TryWriteBytes(destination, value))
            {
                throw new SerializeErrorException("Buffer is too small");
            }
        }

        public override int CalculateLength(int value, IEnumerable<Attribute> attributes = null)
        {
            return sizeof(int);
        }
    }
}
