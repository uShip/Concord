using concord.Output.Dto;
using concord.RazorTemplates.Models;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
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
                .Use<TestRunDirector>();

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

            //For Razor Templating
//            For<ITemplateService>()
//                .Singleton()
//                .Use(() => new TemplateService(
//                    new FluentTemplateServiceConfiguration(x =>
//                        x.IncludeNamespaces(typeof (FancyResults).Namespace)
//                        // .WithEncoding(Encoding.Html)
//                        )));
            For<ITemplateService>().Singleton().Use<TemplateService>();
            For<IResultsTemplateWriter>().Use<ResultsTemplateWriter>();
        }
    }
}