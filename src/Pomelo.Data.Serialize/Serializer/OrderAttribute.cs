using System;

namespace Pomelo.Data.Serialize.Serializer
{
    public class OrderAttribute : Attribute
    {
        public int Order { get; set; }
    }
}
