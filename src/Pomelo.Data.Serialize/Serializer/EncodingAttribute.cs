using System;

namespace Pomelo.Data.Serialize.Serializer
{
    public class EncodingAttribute : Attribute, ISerializeOptionAttribute
    {
        public string Encoding { get; set; }
    }
}
