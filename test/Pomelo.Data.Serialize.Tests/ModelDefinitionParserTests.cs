using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Pomelo.Data.Serialize.Definition;
using Pomelo.Data.Serialize.Tests.Fixtures;
using Newtonsoft.Json;
using Xunit;

namespace Pomelo.Data.Serialize.Tests
{
    public class ModelDefinitionParserTests
    {
        [Fact]
        public void GenerateTest()
        {
            // Arrange
            var resolver = new Serializer.SerializerResolver();
            resolver.AddBasicSerializers();
            var parser = new ModelDefinitionParser(resolver);
            var fileName = "log.def";

            // Act
            var def = parser.GetDefinition<Log>();
            var json = JsonConvert.SerializeObject(def);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            File.WriteAllText(fileName, json);

            // Assert
            Assert.True(File.Exists(fileName));
        }
    }
}
