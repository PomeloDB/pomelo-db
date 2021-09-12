using System;
using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Serializer
{
    public class ULongSerializer : SerializerBase<ulong>
    {
        public override ulong GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null)
        {
            return BitConverter.ToUInt64(value);
        }

        public override void WriteBytes(ulong value, Span<byte> destination, IEnumerable<Attribute> attributes = null)
        {
            if (!BitConverter.TryWriteBytes(destination, value))
            {
                throw new SerializeErrorException("Buffer is too small");
            }
        }

        public override int CalculateLength(ulong value, IEnumerable<Attribute> attributes = null)
        {
            return sizeof(ulong);
        }
    }
}
