using concord.Output.Dto;
using StructureMap.Configuration.DSL;
using concord.Builders;
using concord.Factories;
using concord.Logging;
using concord.Nunit;
using concord.Output;
using concord.Parsers;
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

            For<IRunner>()
                .Use<ProcessRunner>();

            For<ILogger>().Use<Logger>();

            For<IResultMerger>().Use<ResultMerger>();
            For<IResultsWriter>().Use<ResultsWriter>();
            For<IResultsParser>().Use<ResultsParser>();

            For<IRunnerSettingsBuilder>().Use<RunnerSettingsBuilder>();
            For<IProgressDisplay>().Use<ProgressDisplay>();
            For<IResultsStatsWriter>().Use<ResultsStatsWriter>();
            For<IResultsOrderService>().Use<ResultsOrderService>();
            For<IRunStatsCollectionVersioning>().Use<RunStatsCollectionVersioning>();
            For<IRunnerSettings>().Use(() => RunnerSettingsSingleton.Instance);

            For<IHtmlGanttChart>().Use<HtmlGanttChart>();
        }
    }
}