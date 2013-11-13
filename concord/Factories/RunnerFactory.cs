using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StructureMap;
using concord.Builders;
using concord.Configuration;
using concord.Logging;
using concord.Parsers;
using concord.Services;
using FubuCore;

namespace concord.Factories
{
    internal class RunnerFactory : IRunnerFactory
    {
        private readonly ICategoryFinderService _categoryFinderService;
        private readonly IResultsParser _resultsParser;
        private readonly IRunner _runner;

        public RunnerFactory(ICategoryFinderService categoryFinderService,
                             IResultsParser resultsParser,
                             IRunner runner)
        {
            _categoryFinderService = categoryFinderService;
            _resultsParser = resultsParser;
            _runner = runner;
        }

        public IRunner Create(RunnerSettings runnerSettings, string assemblyFileName, string categoriesList = null)
        {
            return Create(runnerSettings, Assembly.LoadFrom(assemblyFileName), categoriesList);
        }

        public IRunner Create(RunnerSettings runnerSettings, Assembly assembly, string categoriesList = null)
        {
            Func<Type, bool> filterTestFixtures = null;
            if (runnerSettings.Namespace != null)
            {
                filterTestFixtures = type => type.Namespace
                                                 .IfNotNull(x =>
                                                            x.StartsWith(runnerSettings.Namespace));
            }

            var categories = _categoryFinderService.FindCategories(assembly, filterTestFixtures);

            var other = categories.Concat(new[] { "Long" });
            var otherFixtures = _categoryFinderService.FindTestFixturesExcludingCategories(assembly, other, filterTestFixtures).ToList();

            if (!categories.Any() && !otherFixtures.Any())
                throw new InvalidOperationException("Cannot run if there are no tests.");

            var categoriesToRun = categoriesList != null
                                      ? categoriesList.Split(',', ';', '\n', '\r')
                                                      .Select(x => x.Trim())
                                                      .Where(x => x.Length > 0)
                                      : new string[0];

            if (runnerSettings.RerunFailedCategories)
            {
                var erroredCategories = _resultsParser.GetErrorsCategories(runnerSettings.ResultsStatsFilepath);
                categoriesToRun = categoriesToRun.Concat(erroredCategories);
            }

            //return new ThreadRunner(_assembly.Location, _featureTypes, ObjectFactory.GetInstance<ILogger>(), _outputPath);
            //return new ProcessRunner(assembly.Location, categories, otherFixtures, categoriesToRun, ObjectFactory.GetInstance<ILogger>(), runnerSettings);
            //TODO: With this, this is no longer a factory...
            _runner.ConfigureRun(assembly.Location, categories, otherFixtures, categoriesToRun, runnerSettings);
            return _runner;
        }
    }
}