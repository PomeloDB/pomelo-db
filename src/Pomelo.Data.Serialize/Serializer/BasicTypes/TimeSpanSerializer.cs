using System;
using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Serializer
{
    public class TimeSpanSerializer : SerializerBase<TimeSpan>
    {
        public override TimeSpan GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null)
        {
            var ticks = BitConverter.ToInt64(value);
            return new TimeSpan(ticks);
        }

        public override void WriteBytes(TimeSpan value, Span<byte> destination, IEnumerable<Attribute> attributes = null)
        {
            if (!BitConverter.TryWriteBytes(destination, value.Ticks))
            {
                throw new SerializeErrorException("Buffer is too small");
            }
        }

        public override int CalculateLength(TimeSpan value, IEnumerable<Attribute> attributes = null)
        {
            return sizeof(long);
        }
    }
}
