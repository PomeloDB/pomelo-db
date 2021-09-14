namespace Pomelo.Data.Dlinq
{
    public class UnsupportedCommandException : DlinqException
    {
        public string Method { get; private set; }
        public int ArgumentsCount { get; private set; }

        public UnsupportedCommandException(string method, int argc) : base($"The command method {method} is not found or the arguments count {argc} is mismatched") 
        {
            Method = method; 
        }
    }
}
