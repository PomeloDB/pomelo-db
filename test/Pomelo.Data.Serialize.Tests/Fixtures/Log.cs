using System;

namespace Pomelo.Data.Serialize.Tests.Fixtures
{
    public class Log
    {
        public DateTime Time { get; set; }

        public string Server { get; set; }

        public int Severity { get; set; }

        public string Message { get; set; }
    }
}
