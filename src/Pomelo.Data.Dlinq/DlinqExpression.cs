using System.Collections.Generic;
using System.Reflection;

namespace Pomelo.Data.Dlinq
{
    public class DlinqExpression
    {
        public object Subject { get; set; }

        public ICollection<DlinqExpressionCommand> Commands { get; set; } = new List<DlinqExpressionCommand>();
    }

    public class DlinqExpressionCommand
    {
        public MethodInfo Method { get; set; }

        public ICollection<object> Arguments { get; set; } = new List<object>();

        public ICollection<object> Parameters { get; set; } = new List<object>();
    }
}
