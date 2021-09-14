using System;

namespace Pomelo.Data.Dlinq
{
    public class InvalidCommandException : Exception
    {
        public string Method { get; private set; }

        public string Error { get; private set; }

        public InvalidCommandException(string method, string error) : base(error)
        {
            Method = method;
            Error = error;
        }
    }
}
