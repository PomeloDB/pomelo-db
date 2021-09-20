namespace Pomelo.Data.Indexing.Btree
{
    public class BTreeInfo
    {
        public long RootIndexNodeLogicAddress { get; set; }
        public long FirstValueNodeLogicAddress { get; set; }
        public int PageSizeMultiplexer { get; set; }
        public int PageReservedSize { get; set; }
    }
}
