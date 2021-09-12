using System;

namespace Pomelo.Data.Dlinq.Tests.Fixtures
{
    public enum Gender
    { 
        Male,
        Female
    }

    public class User
    {
        public string Name { get; set; }

        public int Level { get; set; }

        public Gender Gender { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
