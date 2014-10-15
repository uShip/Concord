using System.IO;
using concord.Configuration;
using NUnit.ConsoleRunner;
using NUnit.Core;
using NUnit.Core.Filters;

namespace concord.Builders.TestRunBuilders
{
    internal class InternalTestRunActionBuilder : ITestRunActionBuilder
    {
        private string _assemblyLocation;
        private string _outputPath;
        private IRunnerSettings _runnerSettings;

        public void Configure(string assemblyLocation, IRunnerSettings runnerSettings)
        {
            _assemblyLocation = assemblyLocation;
            _runnerSettings = runnerSettings;

            _outputPath = runnerSettings.OutputBasePath;
            if (!Directory.Exists(_outputPath))
                Directory.CreateDirectory(_outputPath);
        }

        public TestRunAction BuildTestRunAction(string name, int index, ITestFilter filter)
        {
            //var anyOthers = new CategoryRunner(_assemblyLocation, other, "all", outputPath, countdownEvent);
            var runner = new CategoryRunner(_assemblyLocation, filter, BuildOutputXmlPath(name));
            return new TestRunAction
            {
                Name = name,
                Index = index,
                RunTests = () => runner.Execute()
            };
        }

        public TestRunAction BuildTestRunAction(string name, int index, string testFixture)
        {
            var runner = new CategoryRunner(_assemblyLocation, GetNameFilter(testFixture), BuildOutputXmlPath("fixture-" + testFixture));
            return new TestRunAction
            {
                Name = name,
                Index = index,
                RunTests = () => runner.Execute()
            };
        }

        private ITestFilter GetNameFilter(string featureName)
        {
            var nameFilter = new SimpleNameFilter();
            foreach (string name in TestNameParser.Parse(featureName))
                nameFilter.Add(name);

            return nameFilter;
        }

        private string BuildOutputXmlPath(string category)
        {
            return Path.Combine(_runnerSettings.OutputBasePath, string.Format("{0}.xml", category.Replace("?", "")));
        }
    }
}