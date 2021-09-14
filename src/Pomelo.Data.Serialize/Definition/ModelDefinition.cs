using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Definition
{
    public class ModelDefinition
    {
        public string Name { get; set; }

        public IEnumerable<ModelPropertyDefinition> Properties { get; internal set; }
    }
}
