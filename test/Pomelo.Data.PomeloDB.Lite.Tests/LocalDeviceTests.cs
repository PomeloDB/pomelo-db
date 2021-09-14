using System;
using System.IO;
using System.Linq;
using Pomelo.Data.Serialize.Definition;
using Newtonsoft.Json;
using Xunit;

namespace Pomelo.Data.PomeloDB.Lite.Tests
{
    public class LocalDeviceTests
    {
        [Fact]
        public void ReadWriteTest()
        {
            using (var db = new PomeloDBLite())
            {
                // Arrange
                EnsureDeleted("test1");
                var def = @"
{""Name"":""Log"",""Properties"":[{""Name"":""Time"",""ClrType"":""System.DateTime, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"",""Length"":-1,""Order"":2147483647,""IsEnumerable"":false,""Attributes"":{}},{""Name"":""Server"",""ClrType"":""System.String, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"",""Length"":-1,""Order"":2147483647,""IsEnumerable"":false,""Attributes"":{}},{""Name"":""Severity"",""ClrType"":""System.Int32, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"",""Length"":4,""Order"":2147483647,""IsEnumerable"":false,""Attributes"":{}},{""Name"":""Message"",""ClrType"":""System.String, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"",""Length"":-1,""Order"":2147483647,""IsEnumerable"":false,""Attributes"":{}}]}
";
                db.CreateCollection("test1/test1.fdb", JsonConvert.DeserializeObject<ModelDefinition>(def));
                var collection = db.GetCollection("test1/test1.fdb");
                var device = db.GetCollectionDevice("test1/test1.fdb");
                device.Insert<Fixtures.Log>(new Fixtures.Log
                {
                    Message = "Hello World",
                    Server = "127.0.0.1",
                    Severity = 3,
                    Time = DateTime.UtcNow
                });

                device.Insert<Fixtures.Log>(new Fixtures.Log
                {
                    Message = "Second Message",
                    Server = "127.0.0.1",
                    Severity = 2,
                    Time = DateTime.UtcNow
                });

                device.Insert<Fixtures.Log>(new Fixtures.Log
                {
                    Message = "Third Message",
                    Server = "127.0.0.1",
                    Severity = 2,
                    Time = DateTime.UtcNow
                });

                // Act
                var result = db.ExecuteSingleCommand(@"
test1/test1.fdb
| where Severity == 2
| where Message.Contains(""Message"")");
                var wrapped = Enumerable.ToList(result);

                // Assert
                Assert.Equal(2, wrapped.Count);
            }
        }

        [Fact]
        public void ReadTest()
        {
            using (var db = new PomeloDBLite())
            {
                // Arrange
                var collection = db.GetCollection("Fixtures/TestDB/test1.fdb");

                // Act
                var result = db.ExecuteSingleCommand(@"
test1/test1.fdb
| where Severity == 2
| where Message.Contains(""Message"")");
                var wrapped = Enumerable.ToList(result);

                // Assert
                Assert.Equal(2, wrapped.Count);
            }
        }

        private void EnsureDeleted(string folder)
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            Directory.CreateDirectory(folder);
        }
    }
}