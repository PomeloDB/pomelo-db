using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Pomelo.Data.Serialize.Tests
{
    public class SerializingTests
    {
        [Fact]
        public void CalculateLengthTests()
        {
            // Arrange
            var obj = new Fixtures.TestClass 
            {
                Name = "Yuko",
                Items = new List<string> 
                {
                    "123",
                    "4567",
                    "89012"
                },
                Age = 12
            };
            var resolver = new Serializer.SerializerResolver();
            resolver.AddBasicSerializers();
            var parser = new Definition.ModelDefinitionParser(resolver);
            var serializer = new Serializer.Serializer<Fixtures.TestClass>(parser, resolver);

            // Act
            var len = serializer.CalculateLength(obj);

            // Assert
            Assert.Equal(4 + 4 + 4 + 4 + 3 + 4 + 4 + 4 + 5 + 4, len);
        }

        [Fact]
        public void SerializeTests()
        {
            // Arrange
            var obj = new Fixtures.TestClass
            {
                Name = "Yuko",
                Items = new List<string>
                {
                    "123",
                    "4567",
                    "89012"
                },
                Age = 12
            };
            var resolver = new Serializer.SerializerResolver();
            resolver.AddBasicSerializers();
            var parser = new Definition.ModelDefinitionParser(resolver);
            var serializer = new Serializer.Serializer<Fixtures.TestClass>(parser, resolver);

            // Act
            var len = serializer.CalculateLength(obj);
            var buffer = new Span<byte>(new byte[len]);
            serializer.Serialize(obj, buffer);

            // Assert
            Assert.Equal(4, BitConverter.ToInt32(buffer));
            Assert.Equal("Yuko", Encoding.UTF8.GetString(buffer.Slice(4, 4)));
            Assert.Equal(3, BitConverter.ToInt32(buffer.Slice(8)));
            Assert.Equal(3, BitConverter.ToInt32(buffer.Slice(12)));
            Assert.Equal("123", Encoding.UTF8.GetString(buffer.Slice(16, 3)));
            Assert.Equal(4, BitConverter.ToInt32(buffer.Slice(19)));
            Assert.Equal("4567", Encoding.UTF8.GetString(buffer.Slice(23, 4)));
            Assert.Equal(5, BitConverter.ToInt32(buffer.Slice(27)));
            Assert.Equal("89012", Encoding.UTF8.GetString(buffer.Slice(31, 5)));
            Assert.Equal(12, BitConverter.ToInt32(buffer.Slice(36)));
        }

        [Fact]
        public void SerializeWithOrderTests()
        {
            // Arrange
            var obj = new Fixtures.TestClass2
            {
                Name = "Yuko",
                Items = new List<string>
                {
                    "123",
                    "4567",
                    "89012"
                },
                Age = 12
            };
            var resolver = new Serializer.SerializerResolver();
            resolver.AddBasicSerializers();
            var parser = new Definition.ModelDefinitionParser(resolver);
            var serializer = new Serializer.Serializer<Fixtures.TestClass2>(parser, resolver);

            // Act
            var len = serializer.CalculateLength(obj);
            var buffer = new Span<byte>(new byte[len]);
            serializer.Serialize(obj, buffer);

            // Assert
            Assert.Equal(12, BitConverter.ToInt32(buffer));
        }

        [Fact]
        public void DeserializeTests()
        {
            // Arrange
            var obj = new Fixtures.TestClass
            {
                Name = "Yuko",
                Items = new List<string>
                {
                    "123",
                    "4567",
                    "89012"
                },
                Age = 12
            };
            var resolver = new Serializer.SerializerResolver();
            resolver.AddBasicSerializers();
            var parser = new Definition.ModelDefinitionParser(resolver);
            var serializer = new Serializer.Serializer<Fixtures.TestClass>(parser, resolver);
            var len = serializer.CalculateLength(obj);
            var buffer = new Span<byte>(new byte[len]);
            serializer.Serialize(obj, buffer);
            var deserializer = new Serializer.Deserializer(parser, resolver);
            var definitions = parser.GetDefinition(obj);

            // Act
            var deserializedObject = deserializer.Deserialize(buffer, definitions);

            // Assert
            Assert.NotNull(deserializedObject);
        }
    }
}
