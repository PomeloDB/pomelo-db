using System; 

namespace Pomelo.Data.Serialize.Serializer
{
    public class FixedLengthAttribute : Attribute
    {
        public int Length { get; set; }
    }
}
