using System.Collections.Generic;
using Pomelo.Data.Faster;
using Pomelo.Data.Serialize.Definition;
using Pomelo.Data.Serialize.Serializer;

namespace Pomelo.Data.PomeloDB.Lite
{
    public class FasterEnumerable<T> : FasterEnumerable, IEnumerable<T>
    {
        private FasterLogScanIterator _fasterLogScanIterator;
        private Deserializer _deserializer;
        private ModelDefinition _definition;

        public FasterEnumerable(
            FasterLogScanIterator fasterLogScanIterator, 
            Deserializer deserializer,
            ModelDefinition definition) :base(fasterLogScanIterator, deserializer, definition)
        {
            _fasterLogScanIterator = fasterLogScanIterator;
            _deserializer = deserializer;
            _definition = definition;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new FasterEnumerator<T>(_fasterLogScanIterator, _deserializer, _definition);
        }
    }
}
