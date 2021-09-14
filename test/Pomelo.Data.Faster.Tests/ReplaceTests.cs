using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Pomelo.Data.Faster.Tests
{
    public class ReplaceTests
    {
        [Fact]
        public async Task ReplaceFullTest()
        {
            EnsureDeleted("test1");

            using (var log = Devices.CreateLogDevice("test1\\test1.fdb"))
            {
                // Arrange
                var fasterLog = new FasterLog(new FasterLogSettings { LogDevice = log });
                var addresses = new List<long>(10);
                for (var i = 0; i < 10; ++i)
                {
                    var addr = fasterLog.Enqueue(BitConverter.GetBytes(i));
                    addresses.Add(addr);
                }

                fasterLog.Commit();

                // Act
                fasterLog.UnsafeReplace(BitConverter.GetBytes(100), addresses[1]);

                // Assert
                var expected = new List<int> { 0, 100, 2, 3, 4, 5, 6, 7, 8, 9 };
                var iter = fasterLog.Scan(fasterLog.BeginAddress, fasterLog.TailAddress);
                var actual = new List<int>(10);
                await foreach (var item in iter.GetAsyncEnumerable())
                {
                    actual.Add(BitConverter.ToInt32(item.entry));
                }
                Assert.True(expected.SequenceEqual(actual));
            }
        }
        
        [Fact]
        public async Task ReplacePartialTest()
        {
            EnsureDeleted("test2");

            using (var log = Devices.CreateLogDevice("test2\\test2.fdb"))
            {
                // Arrange
                var fasterLog = new FasterLog(new FasterLogSettings { LogDevice = log });
                var addresses = new List<long>(10);
                for (var i = 0; i < 10; ++i)
                {
                    var entry = new byte[8];
                    BitConverter.TryWriteBytes(new Span<byte>(entry), i);
                    BitConverter.TryWriteBytes(new Span<byte>(entry).Slice(4), i);
                    var addr = fasterLog.Enqueue(entry);
                    addresses.Add(addr);
                }

                fasterLog.Commit();

                // Act
                fasterLog.UnsafeReplace(BitConverter.GetBytes(100), addresses[1]);

                // Assert
                var expected = new List<(int, int)> { (0, 0), (100, 1), (2, 2), (3, 3), (4, 4), (5, 5), (6, 6), (7, 7), (8, 8), (9, 9) };
                var iter = fasterLog.Scan(fasterLog.BeginAddress, fasterLog.TailAddress);
                var actual = new List<(int, int)>(10);
                await foreach (var item in iter.GetAsyncEnumerable())
                {
                    Assert.Equal(8, item.entryLength);
                    actual.Add((BitConverter.ToInt32(item.entry), BitConverter.ToInt32(new Span<byte>(item.entry).Slice(4))));
                }
                Assert.True(expected.SequenceEqual(actual));
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
