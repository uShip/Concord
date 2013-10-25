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

namespace concord.Factories
{
    internal class RunnerFactory : IRunnerFactory
    {
        private readonly ICategoryFinderService _categoryFinderService;
        private readonly IResultsParser _resultsParser;

        public RunnerFactory(ICategoryFinderService categoryFinderService, IResultsParser resultsParser)
        {
            _categoryFinderService = categoryFinderService;
            _resultsParser = resultsParser;
        }

        public IRunner Create(RunnerSettings runnerSettings, string assemblyFileName, bool rerunFailedCategories = false, string categoriesList = null)
        {
            return Create(runnerSettings, Assembly.LoadFrom(assemblyFileName), rerunFailedCategories, categoriesList);
        }

        public IRunner Create(RunnerSettings runnerSettings, Assembly assembly, bool rerunFailedCategories = false, string categoriesList = null)
        {
            var fixtures = _categoryFinderService.FindCategories(assembly);

            if (!fixtures.Any())
                throw new InvalidOperationException("Cannot run if there are no tests.");

            var categoriesToRun = categoriesList != null
                                    ? categoriesList.Split(',', ';').Select(x => x.Trim())
                                    : new string[0];

            if (rerunFailedCategories)
            {
                var erroredCategories = _resultsParser.GetErrorsCategories(runnerSettings.ResultsStatsFilepath);
                categoriesToRun = categoriesToRun.Concat(erroredCategories);
            }

            //return new ThreadRunner(_assembly.Location, _featureTypes, ObjectFactory.GetInstance<ILogger>(), _outputPath);
            return new ProcessRunner(assembly.Location, fixtures, categoriesToRun, ObjectFactory.GetInstance<ILogger>(), runnerSettings);
        }
    }
}