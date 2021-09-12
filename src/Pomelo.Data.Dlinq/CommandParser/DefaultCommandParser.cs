using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;

namespace Pomelo.Data.Dlinq.CommandParser
{
    public class DefaultCommandParser : ICommandParser
    {
        public static Regex ParameterRegex = new Regex("@[a-zA-Z0-9_]{1,}");

        private static DynamicLinqStaticMethod[] methods = typeof(DynamicQueryableExtensions)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Select(x => new DynamicLinqStaticMethod 
            {
                Method = x,
                Parameters = x.GetParameters()
            })
            .ToArray();

        public virtual string Command => null;

        public virtual DlinqExpressionCommand BuildCommand(string commandText, QueryContext context, IDictionary<string, object> parameters = null)
        {
            var cmd = new DlinqExpressionCommand();
            var methodName = GetMethodName(commandText);
            var arguments = GetMethodArgsTexts(commandText);

            var serializedArguments = SerializeArguments(arguments, context, out var serializedParameters, parameters);
            foreach (var argument in serializedArguments)
            {
                cmd.Arguments.Add(argument);
            }

            foreach (var parameter in serializedParameters)
            {
                cmd.Parameters.Add(parameter);
            }

            var queryMethod = GetMethod(methodName, arguments.Length == 0 ? null : cmd.Arguments.Select(x => x.GetType()).ToArray());
            if (queryMethod == null)
            {
                throw new MethodNotFoundException($"No matched method found or arguments are invalid. Method: {methodName}");
            }

            cmd.Method = queryMethod.Method;
            return cmd;
        }

        protected virtual IEnumerable<object> SerializeArguments(string[] arguments, QueryContext context, out ICollection<object> serializedParameters, IDictionary<string, object> parameters)
        {
            var parameterIndex = 0;
            var ret = new List<object>(arguments.Length);
            serializedParameters = new List<object>();

            foreach (var argument in arguments)
            {
                if (argument.StartsWith("[") && argument.EndsWith("]")) // Is `IEnumerable`
                {
                    var collectionName = argument.TrimStart('[').TrimEnd(']');
                    var collection = context.GetCollection(collectionName);
                    if (collection == null)
                    {
                        throw new CollectionNotFoundException($"The collection {collectionName} is not found");
                    }
                    ret.Add(collection);
                }
                else // Is `String`
                {
                    var matches = ParameterRegex.Matches(argument).Cast<Match>();
                    var parametersMap = BuildSerializedParameterList(argument, ref parameterIndex, serializedParameters, parameters);
                    var argv = SerializeParameters(argument, parametersMap);
                    ret.Add(argv);
                }
            }

            return ret;
        }

        protected virtual IEnumerable<(int, string)> BuildSerializedParameterList(string argument, ref int index, ICollection<object> parameterCollection, IDictionary<string, object> parameters = null)
        {
            var matches = ParameterRegex.Matches(argument).Cast<Match>();
            var parametersMap = new List<(int, string)>();

            foreach (var match in matches)
            {
                var key = match.Value.TrimStart('@');
                if (parameters == null || !parameters.ContainsKey(key))
                {
                    throw new ParameterMissingException(key);
                }

                parametersMap.Add((index++, key));
                parameterCollection.Add(parameters[key]);
            }

            return parametersMap;
        }

        protected virtual string SerializeParameters(string argv, IEnumerable<(int, string)> parametersMap)
        {
            foreach (var param in parametersMap.OrderByDescending(x => x.Item2))
            {
                argv = argv.Replace($"@{param.Item2}", $"@{param.Item1}");
            }

            return argv;
        }

        protected virtual string GetMethodName(string commandText)
        {
            return QueryContext.GetMethodName(commandText);
        }

        protected virtual string[] GetMethodArgsTexts(string commandText)
        {
            return InternalGetMethodArgsTexts(commandText).ToArray();
        }

        private IEnumerable<string> InternalGetMethodArgsTexts(string commandText)
        {
            var idxSpace = commandText.IndexOf(' ');
            if (idxSpace >= 0)
            {
                var splited = commandText.Substring(commandText.IndexOf(' '))
                      .Trim()
                      .Split(',')
                      .Select(x => x.Trim());

                var balance = 0;
                var isNewScope = false;
                var newExpression = new List<string>();
                foreach (var argv in splited)
                {
                    if (!isNewScope)
                    {
                        if (argv.IndexOf("new(") < 0)
                        {
                            yield return argv;
                            continue;
                        }

                        if (argv.IndexOf("new(") >= 0)
                        {
                            var currentBalance = GetBracketsBalance(argv);
                            if (currentBalance <= 0)
                            {
                                yield return argv;
                                continue;
                            }

                            isNewScope = true;
                            newExpression.Add(argv);
                            balance += GetBracketsBalance(argv);
                            continue;
                        }
                    }
                    else
                    {
                        newExpression.Add(argv);
                        balance += GetBracketsBalance(argv);
                        if (balance <= 0)
                        {
                            isNewScope = false;
                            balance = 0;
                            yield return string.Join(", ", newExpression);
                            newExpression.Clear();
                        }
                    }
                }
            }
        }

        internal static int GetBracketsBalance(string str)
        {
            var balance = 0;

            foreach (var ch in str)
            {
                if (ch == '(')
                {
                    ++balance;
                }
                else if (ch == ')')
                {
                    --balance;
                }
            }

            return balance;
        }

        internal static DynamicLinqStaticMethod GetMethod(string name, Type[] argumentTypes = null)
        {
            if (argumentTypes == null)
            {
                return methods.FirstOrDefault(x => x.Method.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                    && x.Parameters.Where(x => x.Name != "args").Skip(1).Count() == 0
                    && !x.Method.IsGenericMethod);
            }
            else
            {
                return methods.FirstOrDefault(x => x.Method.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                    && IsAssignable(x.Parameters.Where(y => y.Name != "args").Skip(1).Select(y => y.ParameterType).ToArray(), argumentTypes)
                    && !x.Method.IsGenericMethod);
            }
        }

        internal static bool IsAssignable(Type[] def, Type[] actual)
        {
            if (def.Length != actual.Length)
            {
                return false;
            }

            for (var i = 0; i < def.Length; ++i)
            {
                if (!actual[i].IsAssignableTo(def[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
