using System.Collections.Generic;
using Pomelo.Data.Faster;
using Pomelo.Data.Serialize.Definition;
using Pomelo.Data.Serialize.Serializer;

namespace Pomelo.Data.PomeloDB.Lite
{
    public class FasterEnumerator<T> : FasterEnumerator, IEnumerator<T>
    {
        public FasterEnumerator(
            FasterLogScanIterator fasterLogScanIterator,
            Deserializer deserializer,
            ModelDefinition definition) : base(fasterLogScanIterator, deserializer, definition)
        {
        }

        T IEnumerator<T>.Current => (T)this.Current;
    }
}
