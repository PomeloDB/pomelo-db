using System;
using System.Collections;
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
        private Serializer _serializer;
        private ModelDefinitionParser _parser;
        private Dictionary<string, FasterDevice> devices = new Dictionary<string, FasterDevice>(StringComparer.OrdinalIgnoreCase);
        private static Type _enumerableGenericType = typeof(FasterEnumerable<>);

        public SerializerResolver Resolver => _parser.Resolver;

        public PomeloDBLite()
        {
            _parser = new ModelDefinitionParser(new SerializerResolver());
            Resolver.AddBasicSerializers();
            _deserializer = new Deserializer(_parser, Resolver);
            _serializer = new Serializer(_parser, Resolver);
        }

        public PomeloDBLite(ModelDefinitionParser parser)
        {
            _parser = parser;
            Resolver.AddBasicSerializers();
            _deserializer = new Deserializer(parser, Resolver);
            _serializer = new Serializer(_parser, Resolver);
        }

        public void Dispose()
        {
            foreach (var x in devices)
            {
                x.Value.Logger?.Dispose();
                x.Value.Device?.Close();
                x.Value.Device?.Dispose();
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

        public bool IsCollectionExisted(string name)
        {
            if (!File.Exists(name + ".def"))
            {
                return false;
            }

            return true;
        }

        public void CreateCollection(string name, ModelDefinition definition)
        { 
            if (IsCollectionExisted(name))
            {
                throw new InvalidOperationException($"The collection {name} is already existed.");
            }

            File.WriteAllText(name + ".def", JsonConvert.SerializeObject(definition));
        }

        public override IQueryable GetCollection(string name)
        {
            if (!IsCollectionExisted(name))
            {
                throw new FileNotFoundException($"Faster storage device {name} has not been found.");
            }

            if (!devices.ContainsKey(name))
            {
                var device = Devices.CreateLogDevice(name, recoverDevice: true, useIoCompletionPort: true);
                var fasterLog = new FasterLog(new FasterLogSettings { LogDevice = device });
                devices[name] = new FasterDevice { Device = device, Logger = fasterLog, Deserializer = _deserializer, Serializer = _serializer, Parser = _parser };
            }
            var fasterDevice = devices[name].Logger;
            var iter = fasterDevice.Scan(fasterDevice.BeginAddress, fasterDevice.TailAddress);
            var def = LoadDefinitionFromText(File.ReadAllText(name + ".def"));
            var type = _enumerableGenericType.MakeGenericType(Serializer.GetOrCreateTypeFromDefinition(def));
            return ((IEnumerable)Activator.CreateInstance(type, iter, _deserializer, def)).AsQueryable();
        }

        public FasterDevice GetCollectionDevice(string name)
        {
            if (!IsCollectionExisted(name))
            {
                throw new FileNotFoundException($"Faster storage device {name} has not been found.");
            }

            return devices[name];
        }

        private ModelDefinition LoadDefinitionFromText(string text)
        { 
            return JsonConvert.DeserializeObject<ModelDefinition>(text);
        }
    }
}
