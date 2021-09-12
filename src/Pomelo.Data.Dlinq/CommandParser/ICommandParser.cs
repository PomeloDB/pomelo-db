using System.Collections.Generic;

namespace Pomelo.Data.Dlinq.CommandParser
{
    public interface ICommandParser
    {
        string Command { get; }

        DlinqExpressionCommand BuildCommand(string commandText, QueryContext context, IDictionary<string, object> parameters = null);
    }
}
