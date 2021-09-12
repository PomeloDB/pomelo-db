using System;
using System.Collections.Generic;
using Pomelo.Data.Serialize.Dynamic;
using Xunit;

namespace Pomelo.Data.Serialize.Tests
{
    public class DynamicTests
    {
        [Fact]
        public void SimpleTest()
        {
            // Arrange
            var cc = new ClassContainer("PomeloTest");
            var cb = cc.CreateClass("TestClass", "PomeloTest");
            cb.AddProperty<int>("Age");
            cb.AddProperty<List<string>>("Items");

            // Act
            var type = cb.Build();
            var obj = Activator.CreateInstance(type);
            type.GetProperty("Age").SetValue(obj, 18);
            type.GetProperty("Items").SetValue(obj, new List<string>() { "123", "4567" });

            // Assert
            Assert.Equal("TestClass", type.Name);
            Assert.Equal(2, type.GetProperties().Length);
            Assert.NotNull(type.GetMethod("get_Age"));
            Assert.NotNull(type.GetMethod("get_Items"));
            Assert.NotNull(type.GetMethod("set_Age"));
            Assert.NotNull(type.GetMethod("set_Items"));
        }

        [Fact]
        public void CustomizeGetterTest()
        {
            // Arrange
            var cc = new ClassContainer("PomeloTest");
            var cb = cc.CreateClass("TestClass2", "PomeloTest");
            cb.AddProperty<int>("Age", null, getter: (object self) => 
            {
                return 12;
            });
            cb.AddProperty<List<string>>("Items");

            // Act
            var type = cb.Build();
            var obj = Activator.CreateInstance(type);
            type.GetProperty("Age").SetValue(obj, 18);
            type.GetProperty("Items").SetValue(obj, new List<string>() { "123", "4567" });

            // Assert
            Assert.Equal(12, type.InvokeMember("Age", System.Reflection.BindingFlags.GetProperty, null, obj, null, null));
        }

        [Fact]
        public void CustomizeSetterTest()
        {
            // Arrange
            var cc = new ClassContainer("PomeloTest");
            var cb = cc.CreateClass("TestClass3", "PomeloTest");
            cb.AddProperty<int>("Age", null, setter: (object self, object value) =>
            {
                Console.WriteLine(value.ToString());
                Console.WriteLine(self.ToString());
            });
            cb.AddProperty<List<string>>("Items");

            // Act
            var type = cb.Build();
            var obj = Activator.CreateInstance(type);
            type.GetProperty("Age").SetValue(obj, 18);
            type.GetProperty("Items").SetValue(obj, new List<string>() { "123", "4567" });

            // Assert
            Assert.Equal(0, type.GetProperty("Age").GetValue(obj));
        }
    }
}
