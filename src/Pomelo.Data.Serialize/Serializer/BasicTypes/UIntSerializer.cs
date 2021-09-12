using System;
using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Serializer
{
    public class UIntSerializer : SerializerBase<uint>
    {
        public override uint GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null)
        {
            return BitConverter.ToUInt32(value);
        }

        public override void WriteBytes(uint value, Span<byte> destination, IEnumerable<Attribute> attributes = null)
        {
            if (!BitConverter.TryWriteBytes(destination, value))
            {
                throw new SerializeErrorException("Buffer is too small");
            }
        }

        public override int CalculateLength(uint value, IEnumerable<Attribute> attributes = null)
        {
            return sizeof(uint);
        }
    }
}
