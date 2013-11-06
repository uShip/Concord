using System;

namespace concord.Configuration
{
    public class RunnerSettingsBuilder : IRunnerSettingsBuilder
    {
        private RunnerSettings _runnerSettings;

        public RunnerSettingsBuilder SetOutputFolder(string outputBasePath)
        {
            _runnerSettings = new RunnerSettings(outputBasePath);
            return this;
        }

        public RunnerSettingsBuilder PrependFilenames(string prependWith)
        {
            if (_runnerSettings == null) throw new Exception("Must start with SetOutputFolder");

            if (!string.IsNullOrWhiteSpace(prependWith))
                _runnerSettings.PrependFilenames(prependWith);
            return this;
        }

        public RunnerSettingsBuilder SetResultsXmlFilename(string filename)
        {
            _runnerSettings.ResultsXmlFilename = filename;
            return this;
        }

        public RunnerSettingsBuilder SetResultsHtmlReportFilename(string filename)
        {
            _runnerSettings.ResultsHtmlReportFilename = filename;
            return this;
        }

        public RunnerSettingsBuilder SetResultsStatsFilename(string filename)
        {
            _runnerSettings.ResultsStatsFilename = filename;
            return this;
        }

        public RunnerSettingsBuilder SetNamespace(string ns)
        {
            if (!string.IsNullOrEmpty(ns))
                _runnerSettings.Namespace = ns;

            return this;
        }

        public RunnerSettingsBuilder RunUncategorizedTestFixturesParallel(bool runParallel = true)
        {
            _runnerSettings.RunUncategorizedTestFixturesParallel = runParallel;
            return this;
        }

        public RunnerSettings Build()
        {
            if (_runnerSettings == null) throw new Exception("Must start with SetOutputFolder");

            return _runnerSettings;
        }
    }
}