using System;
using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Serializer
{
    public class BoolSerializer : SerializerBase<bool>
    {
        public override bool GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null)
        {
            return value[0] != 0;
        }

        public override void WriteBytes(bool value, Span<byte> destination, IEnumerable<Attribute> attributes = null)
        {
            destination[0] = value ? (byte)0x01 : (byte)0x00;
        }

        public override int CalculateLength(bool value, IEnumerable<Attribute> attributes = null)
        {
            return 1;
        }
    }
}
