using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pomelo.Data.Serialize.Serializer
{
    public abstract class SerializerBase<T> : ISerializer
    {
        private ISerializer _next = null;

        public Type Type => typeof(T);

        ISerializer ISerializer.Next { get => _next; set => _next = value; }

        public abstract T GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes = null);

        public abstract void WriteBytes(T value, Span<byte> destination, IEnumerable<Attribute> attributes = null);

        public virtual int CalculateLength(T value, IEnumerable<Attribute> attributes = null)
        {
            var type = typeof(T);
            var lengthProperty = type.GetProperties(System.Reflection.BindingFlags.Public).Where(x => x.Name == "Length").SingleOrDefault();
            if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                return Marshal.SizeOf<T>();
            }

            throw new NotImplementedException();
        }

        public virtual bool IsLengthValid(int length) => true;

        object ISerializer.GetValue(ReadOnlySpan<byte> value, IEnumerable<Attribute> attributes) => GetValue(value, attributes);

        void ISerializer.WriteBytes(object value, Span<byte> destination, IEnumerable<Attribute> attributes) => WriteBytes((T)value, destination, attributes);

        int ISerializer.CalculateLength(object value, IEnumerable<Attribute> attributes) => ((SerializerBase<T>)this).CalculateLength((T)value, attributes);
    }
}
