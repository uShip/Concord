using StructureMap.Configuration.DSL;
using concord.Factories;
using concord.Logging;
using concord.Nunit;
using concord.Services;

namespace concord.Configuration
{
    public class RunnerRegistry : Registry
    {
        public RunnerRegistry()
        {
            For<ICategoryFinderService>()
                .Use<CategoryFinderService>();
            For<IRunnerFactory>()
                .Use<RunnerFactory>();
            For<ILogger>().Use<Logger>();
            For<IResultMerger>().Use<ResultMerger>();
        }
    }
}