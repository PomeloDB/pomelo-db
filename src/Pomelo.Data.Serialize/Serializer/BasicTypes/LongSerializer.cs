using System;
using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Serializer
{
    public class LongSerializer : SerializerBase<long>
    {
        public override long GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null)
        {
            return BitConverter.ToInt64(value);
        }

        public override void WriteBytes(long value, Span<byte> destination, IEnumerable<Attribute> attributes = null)
        {
            if (!BitConverter.TryWriteBytes(destination, value))
            {
                throw new SerializeErrorException("Buffer is too small");
            }
        }

        public override int CalculateLength(long value, IEnumerable<Attribute> attributes = null)
        {
            return sizeof(long);
        }
    }
}
