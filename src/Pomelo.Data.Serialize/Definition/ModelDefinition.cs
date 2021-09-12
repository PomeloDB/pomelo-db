using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pomelo.Data.Serialize.Definition
{
    public class ModelDefinition
    {
        public string Name { get; set; }

        public string ClrType { get; set; }

        public int Length { get; set; }

        public int Order { get; set; }

        public bool IsEnumerable { get; set; }

        public IDictionary<string, IDictionary<string, object>> Attributes { get; set; }

        internal IEnumerable<Attribute> ActivateAttributes()
        {
            foreach (var attributeType in Attributes.Keys)
            {
                var type = Type.GetType(attributeType);
                if (type == null)
                {
                    continue;
                }

                var attribute = Activator.CreateInstance(type);

                var properties = Attributes[attributeType];
                foreach (var property in type.GetProperties(BindingFlags.Public))
                {
                    property.SetValue(attribute, properties[property.Name]);
                }

                yield return (Attribute)attribute;
            }
        }
    }
}
