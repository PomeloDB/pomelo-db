using System;

namespace Pomelo.Data.Dlinq.Tests.Fixtures
{
    public class Log
    {
        public DateTime Time { get; set; } = DateTime.UtcNow;

        public string Message { get; set; }
    }
}
