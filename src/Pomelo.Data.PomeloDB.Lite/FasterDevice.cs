using System;
using Pomelo.Data.Faster;
using Pomelo.Data.Serialize.Definition;
using Pomelo.Data.Serialize.Serializer;

namespace Pomelo.Data.PomeloDB.Lite
{
    public class FasterDevice
    {
        public IDevice Device { get; set; }

        public FasterLog Logger { get; set; }

        public Serializer Serializer { get; internal set; }

        public Deserializer Deserializer { get; internal set; }

        public ModelDefinitionParser Parser { get; internal set; }
    }

    public static class FasterDeviceExtensions
    {
        public static readonly byte[] DeletedFlag = new byte[] { 0x01 };

        public static void Commit(this FasterDevice self)
        {
            self.Logger.Commit();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="obj"></param>
        /// <returns>Logical Address</returns>
        public static long Insert<T>(this FasterDevice self, T obj)
        {
            var logicalAddress = InsertWithoutCommit(self, obj);
            Commit(self);
            return logicalAddress;
        }

        public static void Delete(this FasterDevice self, long logicalAddress)
        {
            self.Logger.UnsafeReplace(DeletedFlag, logicalAddress);
        }

        public static void Replace(this FasterDevice self, long logicalAddress, ReadOnlySpan<Byte> data)
        {
            self.Logger.UnsafeReplace(DeletedFlag, logicalAddress);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="obj"></param>
        /// <returns>Logical Address</returns>
        public static long InsertWithoutCommit<T>(this FasterDevice self, T obj)
        {
            var length = self.Serializer.CalculateLength(typeof(T), obj);
            var bytes = new Span<byte>(new byte[length + 1]);
            bytes[0] = 0x00;
            self.Serializer.Serialize(obj, bytes.Slice(1));
            return self.Logger.Enqueue(bytes);
        }
    }
}
