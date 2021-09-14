using System.Collections;
using Pomelo.Data.Faster;
using Pomelo.Data.Serialize.Definition;
using Pomelo.Data.Serialize.Serializer;

namespace Pomelo.Data.PomeloDB.Lite
{
    public class FasterEnumerable : IEnumerable
    {
        private FasterLogScanIterator _fasterLogScanIterator;
        private Deserializer _deserializer;
        private ModelDefinition _definition;

        public FasterEnumerable(
            FasterLogScanIterator fasterLogScanIterator, 
            Deserializer deserializer,
            ModelDefinition definition)
        {
            _fasterLogScanIterator = fasterLogScanIterator;
            _deserializer = deserializer;
            _definition = definition;
        }

        public IEnumerator GetEnumerator()
        {
            return new FasterEnumerator(_fasterLogScanIterator, _deserializer, _definition);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new FasterEnumerator(_fasterLogScanIterator, _deserializer, _definition);
        }
    }
}
