using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomelo.Data.Dlinq.CommandParser
{
    public class GroupJoinCommandParser : DefaultCommandParser
    {
        public override string Command => "GroupJoin";

        // Sample: join [items] on outer.Name = inner.UserId into new(outer.Name as Name, inner.Name as Item)
        protected override string[] GetMethodArgsTexts(string commandText)
        {
            var commandBody = commandText.Substring(commandText.IndexOf(' ')).Trim();
            var ret = new List<string>();

            // Parsing collection
            var idxOn = commandBody.IndexOf(" on ", StringComparison.OrdinalIgnoreCase);
            if (idxOn < 0)
            {
                throw new InvalidCommandException(Command, "Join command must have `on` condition in the expression.");
            }
            var collection = commandBody.Substring(0, idxOn).Trim().TrimStart('[').TrimEnd(']');
            ret.Add($"[{collection}]");

            // Parsing condition
            var condition = GetMiddleText(commandText, " on ", " into ").Trim();
            var splitedCondition = condition.Split("==").Select(x => x.Trim());
            if (splitedCondition.Count() != 2)
            {
                throw new InvalidCommandException(Command, $"The `on` condition in Join expression is invalid. Invalid expression: {condition}");
            }

            if (splitedCondition.Any(x => string.Equals(x.Trim(), "outer")))
            {
                ret.Add("it");
            }
            else
            {
                var outerAccessorText = splitedCondition.SingleOrDefault(x => x.StartsWith("outer.", StringComparison.OrdinalIgnoreCase));
                if (outerAccessorText == null)
                {
                    throw new InvalidCommandException(Command, $"The `on` condition in Join expression must have an outer accessor. Invalid expression: {condition}");
                }
                ret.Add(outerAccessorText.Substring("outer.".Length));
            }

            if (splitedCondition.Any(x => string.Equals(x.Trim(), "inner")))
            {
                ret.Add("it");
            }
            else
            {
                var innerAccessorText = splitedCondition.SingleOrDefault(x => x.StartsWith("inner.", StringComparison.OrdinalIgnoreCase));
                if (innerAccessorText == null)
                {
                    throw new InvalidCommandException(Command, $"The `on` condition in Join expression must have an inner accessor. Invalid expression: {condition}");
                }
                ret.Add(innerAccessorText.Substring("inner.".Length));
            }

            // Parsing structure
            var idxInto = commandText.LastIndexOf(" into ", StringComparison.OrdinalIgnoreCase);
            if (idxInto < 0)
            {
                throw new InvalidCommandException(Command, $"Join command must have `into` in the expression.");
            }

            if (idxInto + " into ".Length >= commandText.Length)
            {
                throw new InvalidCommandException(Command, $"The `into` expression in Join command is invalid.");
            }

            var structureText = commandText.Substring(idxInto + " into ".Length);
            ret.Add(structureText);

            return ret.ToArray();
        }

        internal static string GetMiddleText(string src, string start, string end)
        {
            var idxStart = src.IndexOf(start, StringComparison.OrdinalIgnoreCase);
            if (idxStart < 0)
            {
                return null;
            }
            idxStart += start.Length;

            var idxEnd = src.LastIndexOf(end, StringComparison.OrdinalIgnoreCase);
            if (idxEnd < 0)
            {
                return null;
            }

            if (idxStart > idxEnd)
            {
                return null;
            }

            return src.Substring(idxStart, idxEnd - idxStart);
        }
    }
}
