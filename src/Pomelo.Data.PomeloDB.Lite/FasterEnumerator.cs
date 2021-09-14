using System;
using System.Collections;
using Pomelo.Data.Faster;
using Pomelo.Data.Serialize.Definition;
using Pomelo.Data.Serialize.Serializer;

namespace Pomelo.Data.PomeloDB.Lite
{
    public class FasterEnumerator : IEnumerator
    {
        private FasterLogScanIterator _fasterLogScanIterator;
        private Deserializer _deserializer;
        private ModelDefinition _definition;

        private object _current = null;
        private long _logicalAddress = -1;

        public FasterEnumerator(
            FasterLogScanIterator fasterLogScanIterator,
            Deserializer deserializer,
            ModelDefinition definition)
        {
            _fasterLogScanIterator = fasterLogScanIterator;
            _deserializer = deserializer;
            _definition = definition;
        }

        public object Current => _current;

        public long CurrentLogicalAddress => _logicalAddress;

        object IEnumerator.Current => _current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            while (true)
            {
                if (_fasterLogScanIterator.GetNext(out var next, out var length, out var addr))
                {
                    if (next[0] != 0x00) // Is deleted
                    {
                        continue;
                    }

                    _logicalAddress = addr;
                    _current = _deserializer.Deserialize(new Span<byte>(next).Slice(1, length - 1), _definition);
                    return true;
                }

                return false;
            }
        }

        public void Reset()
        {
            _fasterLogScanIterator.Reset();
        }
    }
}
