using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Pomelo.Data.Dlinq.Tests.Fixtures;

namespace Pomelo.Data.Dlinq.Tests
{
    public class QueryTests
    {
        [Fact]
        public void SimpleQueryTest()
        {
            // Arrange
            var context = new TestQueryContext();

            // Act
            var result = context.ExecuteSingleCommand(@"
users
| where Name.Contains(""o"")
| orderby Level");

            // Assert
            Assert.Equal(2, Enumerable.Count(result));
        }

        [Fact]
        public void JoinQueryTest()
        {
            // Arrange
            var context = new TestQueryContext();

            // Act
            var result = context.ExecuteSingleCommand(@"
users
| where Level >= 5
| join [items] on inner.UserId == outer.Name into new(outer.Name as Username, inner.Name as ItemName)");

            // Assert
            Assert.Equal(3, Enumerable.Count(result));
        }

        [Fact]
        public void GroupQueryTest()
        {
            // Arrange
            var context = new TestQueryContext();

            // Act
            var result = context.ExecuteSingleCommand(@"
users
| groupby Level > 5
| select new(Key as LevelGreaterThanFive, Count() as Count)");

            // Assert
            Assert.Equal(2, Enumerable.Count(result));
        }

        [Fact]
        public void DateTimeQueryTest()
        {
            // Arrange
            var context = new TestQueryContext();

            // Act
            var result = context.ExecuteSingleCommand(@"
logs
| where Time >= Convert.ToDateTime(""2021-09-15 00:00"")");

            // Assert
            Assert.Equal(3, Enumerable.Count(result));
        }

        [Fact]
        public void DateTimeQueryWithParameterTest()
        {
            // Arrange
            var context = new TestQueryContext();

            // Act
            var result = context.ExecuteSingleCommand(@"
logs
| where Time >= @time", new Dictionary<string, object> { { "time", Convert.ToDateTime("2021-09-15 00:00") } });

            // Assert
            Assert.Equal(3, Enumerable.Count(result));
        }

        [Fact]
        public void ReverseQueryTest()
        {
            // Arrange
            var context = new TestQueryContext();

            // Act
            var result = context.ExecuteSingleCommand(@"
logs
| orderby Time
| take 2
| reverse");
            var wrapped = DynamicEnumerableExtensions.ToDynamicList(result);

            // Assert
            Assert.True(wrapped[0].Time > wrapped[1].Time);
        }

        [Fact]
        public void TakeQueryWithParameterTest()
        {
            // Arrange
            var context = new TestQueryContext();

            // Act
            var result = context.ExecuteSingleCommand(@"
logs
| take @count", new Dictionary<string, object> { { "count", 3 } });

            // Assert
            Assert.Equal(3, Enumerable.Count(result));
        }

        [Fact]
        public void LastQueryTest()
        {
            // Arrange
            var context = new TestQueryContext();

            // Act
            var result = context.ExecuteSingleCommand(@"
logs
| last");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Convert.ToDateTime("2021/9/30 15:29:00"), result.Time);
        }
    }
}
