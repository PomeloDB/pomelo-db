using System.Collections.Generic;

namespace Pomelo.Data.Dlinq.CommandParser
{
    public class SkipCommandParser : DefaultCommandParser
    {
        public override string Command => "Skip";

        protected override IEnumerable<object> SerializeArguments(string[] arguments, QueryContext context, out ICollection<object> serializedParameters, IDictionary<string, object> parameters)
        {
            var ret = new List<object>();
            serializedParameters = new List<object>();

            foreach (var argv in arguments)
            {
                if (ParameterRegex.IsMatch(argv))
                {
                    var parameterName = argv.TrimStart('@');
                    if (!parameters.ContainsKey(parameterName))
                    {
                        throw new ParameterMissingException(parameterName);
                    }
                    if (parameters[parameterName] is int result)
                    {
                        ret.Add(result);
                    }
                    else
                    {
                        throw new ParameterInvalidException(parameterName);
                    }
                }
                else if (int.TryParse(argv.ToString(), out var result))
                {
                    ret.Add(result);
                }
                else
                {
                    ret.Add(argv);
                }
            }

            return ret;
        }
    }
}
