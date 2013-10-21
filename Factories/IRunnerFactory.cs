using System.Reflection;
using concord.Builders;

namespace concord.Factories
{
    public interface IRunnerFactory
    {
        IRunner Create(string assemblyFileName, string categoriesList = null, string outputPath = null);
        IRunner Create(Assembly assembly, string categoriesList = null, string outputPath = null);
    }
}