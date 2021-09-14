using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Pomelo.Data.Faster;
using Newtonsoft.Json;
using Pomelo.Data.Dlinq;
using Pomelo.Data.Serialize.Definition;
using Pomelo.Data.Serialize.Serializer;

namespace Pomelo.Data.PomeloDB.Lite
{
    public class PomeloDBLite : QueryContext, IDisposable
    {
        private Deserializer _deserializer;

        public SerializerResolver Resolver { get; private set; }

        public PomeloDBLite(ModelDefinitionParser parser)
        {
            Resolver = new SerializerResolver();
            Resolver.AddBasicSerializers();
            _deserializer = new Deserializer(parser, Resolver);
        }

        private Dictionary<string, FasterDevice> devices = new Dictionary<string, FasterDevice>();

        public void Dispose()
        {
            foreach (var x in devices)
            {
                x.Value.Device.Dispose();
            }
        }

        public FasterDevice GetDevice(string name)
        {
            if (!devices.ContainsKey(name))
            {
                return null;
            }

            return devices[name];
        }

        public IQueryable GetCollection(string name, bool createIfNotExist)
        {
            if (!createIfNotExist)
            {
                return GetCollection(name);
            }

            var recover = false;
            if (File.Exists(name + ".0"))
            {
                recover = true;
            }

            var device = Devices.CreateLogDevice(name, recoverDevice: recover, useIoCompletionPort: true);
            var fasterLog = new FasterLog(new FasterLogSettings { LogDevice = device });
            devices[name] = new FasterDevice { Device = device, Logger = fasterLog };
            var iter = fasterLog.Scan(fasterLog.BeginAddress, fasterLog.TailAddress);
            return new FasterEnumerable(iter, _deserializer, LoadDefinitionFromText(File.ReadAllText(name + ".def"))).AsQueryable();
        }

        public override IQueryable GetCollection(string name)
        {
            if (!File.Exists(name + ".0") || !File.Exists(name + ".def"))
            {
                throw new FileNotFoundException($"Faster storage device {name} has not been found.");
            }

            var device = Devices.CreateLogDevice(name, recoverDevice: true, useIoCompletionPort: true);
            var fasterLog = new FasterLog(new FasterLogSettings { LogDevice = device });
            devices[name] = new FasterDevice { Device = device, Logger = fasterLog };
            var iter = fasterLog.Scan(fasterLog.BeginAddress, fasterLog.TailAddress);
            return new FasterEnumerable(iter, _deserializer, LoadDefinitionFromText(File.ReadAllText(name + ".def"))).AsQueryable();
        }

        private ModelDefinition LoadDefinitionFromText(string text)
        { 
            return JsonConvert.DeserializeObject<ModelDefinition>(text);
        }
    }
}
