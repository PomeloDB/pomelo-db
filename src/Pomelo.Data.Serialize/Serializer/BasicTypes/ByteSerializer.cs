using System;
using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Serializer
{
    public class ByteSerializer : SerializerBase<byte>
    {
        public override byte GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null)
        {
            return value[0];
        }

        public override void WriteBytes(byte value, Span<byte> destination, IEnumerable<Attribute> attributes = null)
        {
            destination[0] = value;
        }

        public override int CalculateLength(byte value, IEnumerable<Attribute> attributes = null)
        {
            return sizeof(byte);
        }
    }
}
