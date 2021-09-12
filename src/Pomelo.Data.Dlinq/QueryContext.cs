using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using Pomelo.Data.Dlinq.CommandParser;

namespace Pomelo.Data.Dlinq
{
    public class QueryContext
    {
        private Dictionary<string, ICommandParser> parsers = new Dictionary<string, ICommandParser>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, IQueryable> collections = new Dictionary<string, IQueryable>();

        public QueryContext()
        {
            RegisterBuiltInParsers();
        }

        public void RegisterParser(string method, ICommandParser parser) 
            => parsers[method] = parser;

        public void RegisterCollection(string name, IQueryable collection)
            => collections[name] = collection;

        public void UnregisterCollection(string name)
        { 
            if (collections.ContainsKey(name))
            {
                collections.Remove(name);
            }
        }

        public dynamic ExecuteSingleCommand(string query, IDictionary<string, object> parameters = null)
        {
            var queryBody = BuildDlinqExpression(query, parameters);
            var result = queryBody.Subject;
            foreach (var cmd in queryBody.Commands)
            {
                List<object> arguments = new List<object>(cmd.Arguments.Count + 2);
                arguments.Add(result);
                arguments.AddRange(cmd.Arguments);
                if (cmd.Method.GetParameters().Any(x => x.Name == "args"))
                {
                    arguments.Add(cmd.Parameters.ToArray());
                }
                result = cmd.Method.Invoke(null, BindingFlags.Public | BindingFlags.Static, null, arguments.ToArray(), null);
            }
            return result;
        }

        private PropertyInfo[] _properties;

        internal PropertyInfo[] Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = this.GetType().GetProperties();
                }
                return _properties;
            }
        }

        internal void RegisterBuiltInParsers()
        {
            RegisterParser(string.Empty, new DefaultCommandParser());
            RegisterParser("Join", new JoinCommandParser());
            RegisterParser("GroupJoin", new GroupJoinCommandParser());
            RegisterParser("Skip", new SkipCommandParser());
            RegisterParser("Take", new TakeCommandParser());
        }

        internal ICommandParser GetCommandParser(string method)
        { 
            if (parsers.ContainsKey(method))
            {
                return parsers[method];
            }

            return parsers[string.Empty];
        }

        internal IQueryable GetCollection(string name)
        {
            if (collections.ContainsKey(name))
            {
                return collections[name];
            }

            var property = Properties.SingleOrDefault(x => x.PropertyType.IsAssignableTo(typeof(IQueryable))
                    && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                return null;
            }

            return (IQueryable)property.GetValue(this);
        }

        public DlinqExpression BuildDlinqExpression(string query, IDictionary<string, object> parameters)
        {
            var ret = new DlinqExpression(); 
            var split = query.Split('|')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToArray();

            // Find subject
            var subjectName = split[0].TrimStart('[').TrimEnd(']');
            ret.Subject = GetCollection(subjectName);
            if (ret.Subject == null)
            {
                throw new CollectionNotFoundException($"The collection [{subjectName}] is not found");
            }

            // Build Command
            for (var i = 1; i < split.Length; ++i)
            {
                var method = GetMethodName(split[i]);
                var parser = GetCommandParser(method);
                var command = parser.BuildCommand(split[i], this, parameters);
                ret.Commands.Add(command);
            }

            return ret;
        }

        internal static string GetMethodName(string commandText)
        {
            var idxSpace = commandText.IndexOf(' ');
            if (idxSpace < 0)
            {
                return commandText;
            }

            return commandText.Substring(0, commandText.IndexOf(' ')).Trim();
        }
    }
}
