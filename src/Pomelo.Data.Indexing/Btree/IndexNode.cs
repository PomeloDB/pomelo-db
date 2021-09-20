using System.Collections.Generic;

namespace Pomelo.Data.Indexing.Btree
{
    public class IndexNode<T>
    {
        public bool IsLeaf { get; set; }

        public long ParentLogicAddress { get; set; }

        public long ValueNodeLogicAddress { get; set; }

        public List<KeyAddressPair<T>> Index { get; set; }
    }
}
