using System.Collections.Generic;
using Pomelo.Data.Serialize.Serializer;

namespace Pomelo.Data.Serialize.Tests.Fixtures
{
    public class TestClass
    {
        public string Name { get; set; }

        public List<string> Items { get; set; }

        public int Age { get; set; }
    }

    public class TestClass2
    {
        public string Name { get; set; }

        public List<string> Items { get; set; }

        [Order(Order = 1)]
        public int Age { get; set; }
    }
}
