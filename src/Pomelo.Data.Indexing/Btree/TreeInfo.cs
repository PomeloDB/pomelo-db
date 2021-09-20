using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pomelo.Data.Indexing.Btree
{
    public class TreeInfo
    {
        public long RootNodeLogicalAddress { get; set; }

        public int PageSizeMultiplexer { get; set; }

        public int PageReservedSize { get; set; }
    }
}
