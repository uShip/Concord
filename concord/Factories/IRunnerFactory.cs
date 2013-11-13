using System.Reflection;
using concord.Builders;
using concord.Configuration;

namespace concord.Factories
{
    public interface IRunnerFactory
    {
        IRunner Create(RunnerSettings runnerSettings, string assemblyFileName, string categoriesList = null);
        IRunner Create(RunnerSettings runnerSettings, Assembly assembly, string categoriesList = null);
    }
}