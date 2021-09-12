namespace Pomelo.Data.Dlinq
{
    public class CollectionNotFoundException : DlinqException
    {
        public CollectionNotFoundException(string error) : base(error) { }
    }
}
