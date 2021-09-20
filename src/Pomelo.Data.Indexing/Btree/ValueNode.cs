using System.Collections.Generic;

namespace Pomelo.Data.Indexing.Btree
{
    public class ValueNode<T>
    {
        public long ParentLogicalAddress { get; set; }
        public long NextLogicalAddress { get; set; }
        public List<KeyAddressPair<T>> KeyValues { get; set; }
    }
}
