using System.Reflection;
using System.Reflection.Emit; 

namespace Pomelo.Data.Serialize.Dynamic
{
    public class ClassContainer
    {
        AssemblyBuilder _assemblyBuilder;

        public AssemblyBuilder AssemblyBuilder => _assemblyBuilder;

        public ClassContainer(string assemblyName)
        {
            var _assemblyName = new AssemblyName(assemblyName);
            _assemblyBuilder = AssemblyBuilder
                .DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.RunAndCollect);
        }

        public ClassBuilder CreateClass(string className, string @namespace = null)
        {
            return new ClassBuilder(this, className, @namespace);
        }

        public void Build()
        {
        }
    }
}
