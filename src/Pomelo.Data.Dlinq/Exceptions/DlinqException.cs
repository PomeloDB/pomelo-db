using System;

namespace Pomelo.Data.Dlinq
{
    public class DlinqException : Exception
    {
        public DlinqException(string error) : base(error)
        { 
        }
    }
}
