using System;
using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Serializer
{
    public class ShortSerializer : SerializerBase<short>
    {
        public override short GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null)
        {
            return BitConverter.ToInt16(value);
        }

        public override void WriteBytes(short value, Span<byte> destination, IEnumerable<Attribute> attributes = null)
        {
            if (!BitConverter.TryWriteBytes(destination, value))
            {
                throw new SerializeErrorException("Buffer is too small");
            }
        }

        public override int CalculateLength(short value, IEnumerable<Attribute> attributes = null)
        {
            return sizeof(short);
        }
    }
}
