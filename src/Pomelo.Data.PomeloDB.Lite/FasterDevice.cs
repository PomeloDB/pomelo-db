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
        public static void Commit(this FasterDevice self)
        {
            self.Logger.Commit();
        }

        public static void Insert<T>(this FasterDevice self, T obj)
        {
            InsertWithoutCommit(self, obj);
            Commit(self);
        }

        public static void InsertWithoutCommit<T>(this FasterDevice self, T obj)
        {
            var length = self.Serializer.CalculateLength(typeof(T), obj);
            var bytes = new Span<byte>(new byte[length + 1]);
            bytes[0] = 0x00;
            self.Serializer.Serialize(obj, bytes.Slice(1));
            self.Logger.Enqueue(bytes);
        }
    }
}
