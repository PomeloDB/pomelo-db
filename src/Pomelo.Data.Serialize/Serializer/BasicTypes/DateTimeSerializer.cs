using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomelo.Data.Serialize.Serializer
{
    public class DateTimeSerializer : SerializerBase<DateTime>
    {
        public override DateTime GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null)
        {
            var ticks = BitConverter.ToInt64(value);
            var dateTimeKindAttribute = attributes.SingleOrDefault(x => x is DateTimeKindAttribute);
            DateTimeKind kind = default;
            if (dateTimeKindAttribute is DateTimeKindAttribute attribute)
            {
                kind = attribute.DateTimeKind;
            }
            return new DateTime(ticks, kind);
        }

        public override void WriteBytes(DateTime value, Span<byte> destination, IEnumerable<Attribute> attributes = null)
        {
            if (!BitConverter.TryWriteBytes(destination, value.Ticks))
            {
                throw new SerializeErrorException("Buffer is too small");
            }
        }

        public override int CalculateLength(DateTime value, IEnumerable<Attribute> attributes = null)
        {
            return sizeof(long);
        }

        public override int DefaultLength => sizeof(long);
    }
}
