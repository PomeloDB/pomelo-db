namespace Pomelo.Data.Dlinq
{
    public class ParameterMissingException : DlinqException
    {
        public string Parameter { get; private set; }

        public ParameterMissingException(string parameter) : base($"The parameter {parameter} is missing") { }
    }
}
