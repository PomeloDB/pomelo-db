using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomelo.Data.Dlinq.Tests.Fixtures
{
    public class TestQueryContext : QueryContext
    {
        public IQueryable<User> Users { get; set; }

        public IQueryable<Item> Items { get; set; }

        public IQueryable<Log> Logs { get; set; }

        public TestQueryContext()
        {
            Users = new List<User> 
            {
                new User { Name = "Tom", Gender = Gender.Male, Level = 5 },
                new User { Name = "Windy", Gender = Gender.Female, Level = 7 },
                new User { Name = "Lucy", Gender = Gender.Female, Level = 1 },
                new User { Name = "Bob", Gender = Gender.Male, Level = 3 },
                new User { Name = "Alex", Gender = Gender.Male, Level = 6 },
                new User { Name = "Cat", Gender = Gender.Female, Level = 2 },
            }.AsQueryable();

            Items = new List<Item>
            {
                new Item { Name = "Item #1", UserId = "Tom" },
                new Item { Name = "Item #2", UserId = "Tom" },
                new Item { Name = "Item #3", UserId = "Alex" },
                new Item { Name = "Item #4", UserId = "Cat" },
                new Item { Name = "Item #5", UserId = "Lucy" },
                new Item { Name = "Item #6", UserId = "Lucy" },
            }.AsQueryable();

            Logs = new List<Log>
            {
                new Log { Message = Guid.NewGuid().ToString(), Time = Convert.ToDateTime("2021-09-01 05:29") },
                new Log { Message = Guid.NewGuid().ToString(), Time = Convert.ToDateTime("2021-09-04 11:15") },
                new Log { Message = Guid.NewGuid().ToString(), Time = Convert.ToDateTime("2021-09-15 19:12") },
                new Log { Message = Guid.NewGuid().ToString(), Time = Convert.ToDateTime("2021-09-24 02:56") },
                new Log { Message = Guid.NewGuid().ToString(), Time = Convert.ToDateTime("2021-09-30 15:29") },
            }
            .AsQueryable();
        }
    }
}
