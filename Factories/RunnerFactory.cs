using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StructureMap;
using concord.Builders;
using concord.Configuration;
using concord.Logging;
using concord.Services;

namespace concord.Factories
{
    internal class RunnerFactory : IRunnerFactory
    {
        private readonly ICategoryFinderService _categoryFinderService;
        private readonly ISettings _settings;


        public RunnerFactory(ICategoryFinderService categoryFinderService)
        {
            _settings = Settings.Instance;
            _categoryFinderService = categoryFinderService;
        }

        public IRunner Create(string assemblyFileName, string categoriesList = null, string outputPath = null)
        {
            return Create(Assembly.LoadFrom(assemblyFileName), categoriesList, outputPath);
        }

        public IRunner Create(Assembly assembly, string categoriesList = null, string outputPath = null)
        {
            var fixtures = _categoryFinderService.FindCategories(assembly);

            if (!fixtures.Any())
                throw new InvalidOperationException("Cannot run if there are no tests.");

            var categoriesToRun = categoriesList != null
                                    ? categoriesList.Split(',', ';').Select(x => x.Trim())
                                    : new string[0];

            //return new ThreadRunner(_assembly.Location, _featureTypes, ObjectFactory.GetInstance<ILogger>(), _outputPath);
            return new ProcessRunner(assembly.Location, fixtures, categoriesToRun, ObjectFactory.GetInstance<ILogger>(), outputPath);
        }
    }
}