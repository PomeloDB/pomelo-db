using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using FASTER.core;
using Pomelo.Data.Dlinq;

namespace Pomelo.Data.PomeloDB.Lite
{
    public class PomeloDBLite : QueryContext, IDisposable
    {
        private Dictionary<string, IDevice> devices = new Dictionary<string, IDevice>();

        public void Dispose()
        {

        }

        protected override IQueryable GetCollection(string name)
        {
            if (!File.Exists(name + ".0") || !File.Exists(name + ".def"))
            {
                throw new FileNotFoundException($"Faster storage device {name} has not been found.");
            }

            return null;
        }

        private IQueryable GetFasterStorageDevice(string path)
        {
            var device = Devices.CreateLogDevice(path, recoverDevice: true, useIoCompletionPort: true);
            devices[path] = device;
            var fasterLog = new FasterLog(new FasterLogSettings { LogDevice = device });

        }
    }
}
