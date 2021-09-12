using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pomelo.Data.Serialize.Serializer
{
    public class StringSerializer : SerializerBase<string>
    {
        public override bool IsFlexLength => true;

        public override string GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null)
        {
            var encodingAttribute = attributes.SingleOrDefault(x => x is EncodingAttribute);
            Encoding encoding = Encoding.UTF8;
            if (encodingAttribute != null)
            {
                encoding = Encoding.GetEncoding((encodingAttribute as EncodingAttribute).Encoding);
            }
            return encoding.GetString(value);
        }

        public override void WriteBytes(string value, Span<byte> destination, IEnumerable<Attribute> attributes = null)
        {
            var encodingAttribute = attributes.SingleOrDefault(x => x is EncodingAttribute);
            Encoding encoding = Encoding.UTF8;
            if (encodingAttribute != null)
            {
                encoding = Encoding.GetEncoding((encodingAttribute as EncodingAttribute).Encoding);
            }
            encoding.GetBytes(value, destination);
        }

        public override int CalculateLength(string value, IEnumerable<Attribute> attributes = null)
        {
            var encodingAttribute = attributes.SingleOrDefault(x => x is EncodingAttribute);
            Encoding encoding = Encoding.UTF8;
            if (encodingAttribute != null)
            {
                encoding = Encoding.GetEncoding((encodingAttribute as EncodingAttribute).Encoding);
            }
            return encoding.GetByteCount(value);
        }
    }
}
