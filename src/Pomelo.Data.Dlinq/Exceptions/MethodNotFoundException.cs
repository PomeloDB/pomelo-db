namespace Pomelo.Data.Dlinq
{
    public class MethodNotFoundException : DlinqException
    {
        public MethodNotFoundException(string error) : base(error) { }
    }
}
