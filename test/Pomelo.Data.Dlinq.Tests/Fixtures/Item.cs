using System;

namespace Pomelo.Data.Dlinq.Tests.Fixtures
{
    public class Item
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserId { get; set; }

        public string Name { get; set; }
    }
}
