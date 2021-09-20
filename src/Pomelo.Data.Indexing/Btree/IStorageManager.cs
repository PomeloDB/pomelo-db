namespace Pomelo.Data.Indexing.Btree
{
    internal interface IStorageManager<T>
    {
        IndexNode<T> GetIndexNode(long LogicalAddress);
        ValueNode<T> GetValueNode(long LogicalAddress);
        void StoreIndexNode(IndexNode<T> node);
        void StoreValueNode(IndexNode<T> node);
        void StoreTreeInfo(TreeInfo treeInfo);
        int GetTLength();
        long Allocate(int pageSizeMultiplexer, int reserve); // Actual Size = 4096 * multiplexer - reserve
    }
}
