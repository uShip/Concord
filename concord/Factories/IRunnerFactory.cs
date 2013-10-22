using System.Reflection;
using concord.Builders;

namespace concord.Factories
{
    public interface IRunnerFactory
    {
        IRunner Create(string assemblyFileName, bool rerunFailedCategories = false, string categoriesList = null, string outputPath = null);
        IRunner Create(Assembly assembly, bool rerunFailedCategories = false, string categoriesList = null, string outputPath = null);
    }
}