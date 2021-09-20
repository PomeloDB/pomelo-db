using System.IO;
using Pomelo.Data.Faster;
using Pomelo.Data.Serialize.Serializer;

namespace Pomelo.Data.Indexing.Btree
{
    public class BTree<T>
    {
        private IDevice device;
        private FasterLog log;
        private ISerializer serializer;
        public const int TreeInfoSize = 2000;

        public string FilePath { get; private set; }

        public BTree(string path)
        {
            FilePath = path;
            device = Devices.CreateLogDevice(path, useIoCompletionPort: true, recoverDevice: true);
            log = new FasterLog(new FasterLogSettings { LogDevice = device });

            if (File.Exists(path + ".pbt"))
            {
                LoadExistedBTree(path);
            }
            else
            {
                CreateNewBTree(path);
            }
        }

        private void LoadExistedBTree(string path)
        {
        }

        private void CreateNewBTree(string path)
        { 
        
        }

        internal IStorageManager<T> storageManager;

        public TreeInfo TreeInfo { get; set; }

        public void Add(T key, long value)
        { 
            
        }

        public long? Find(T key)
        { 
        
        }

        public void Remove(T key)
        { 
        
        }
    }
}
