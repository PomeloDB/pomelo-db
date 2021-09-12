using System;

namespace Pomelo.Data.Serialize.Serializer
{
    public class SerializeErrorException : Exception
    {
        public SerializeErrorException(string error) : base(error)
        { }

        public string Property { get; set; }
    }
}
