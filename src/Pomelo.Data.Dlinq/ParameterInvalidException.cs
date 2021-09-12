namespace Pomelo.Data.Dlinq
{
    public class ParameterInvalidException : DlinqException
    {
        public string Parameter { get; private set; }

        public ParameterInvalidException(string parameter) : base($"The parameter {parameter} is invalid") { }
    }
}
