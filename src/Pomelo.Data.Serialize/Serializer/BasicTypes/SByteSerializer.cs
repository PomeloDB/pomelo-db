using System;
using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Serializer
{
    public class SByteSerializer : SerializerBase<sbyte>
    {
        public override sbyte GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null)
        {
            return unchecked((sbyte)value[0]);
        }

        public override void WriteBytes(sbyte value, Span<byte> destination, IEnumerable<Attribute> attributes = null)
        {
            destination[0] = unchecked((byte)value);
        }

        public override int CalculateLength(sbyte value, IEnumerable<Attribute> attributes = null)
        {
            return sizeof(sbyte);
        }
    }
}
