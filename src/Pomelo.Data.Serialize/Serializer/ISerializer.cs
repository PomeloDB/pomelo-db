using System;
using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Serializer
{
    public interface ISerializer
    {
        ISerializer Next { get; internal set; }
        Type Type { get; }
        void WriteBytes(object value, Span<byte> destination, IEnumerable<Attribute> attributes = null);
        object GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null);
        int CalculateLength(object value, IEnumerable<Attribute> attributes = null);
        bool IsFlexLength { get; }
        int DefaultLength { get; }
        bool IsLengthValid(int length);
    }
}
