
namespace Pomelo.Data.Dlinq.CommandParser
{
    public class OrderByCommandParser : DefaultCommandParser
    {
        public override string Command => "OrderBy";

        protected override string[] GetMethodArgsTexts(string commandText)
        {
            return new string[] { commandText.Substring(commandText.IndexOf(' ')).Trim() };
        }
    }
}
