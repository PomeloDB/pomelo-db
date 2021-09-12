using System;
using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Serializer
{
    public class UShortSerializer : SerializerBase<ushort>
    {
        public override ushort GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null)
        {
            return BitConverter.ToUInt16(value);
        }

        public override void WriteBytes(ushort value, Span<byte> destination, IEnumerable<Attribute> attributes = null)
        {
            if (!BitConverter.TryWriteBytes(destination, value))
            {
                throw new SerializeErrorException("Buffer is too small");
            }
        }

        public override int CalculateLength(ushort value, IEnumerable<Attribute> attributes = null)
        {
            return sizeof(ushort);
        }
    }
}
