using System;

namespace concord.Configuration
{
    public class RunnerSettingsBuilder : IRunnerSettingsBuilder, IParallelizationSettingsBuilder, ITestRunActionSettingsBuilder
    {
        private RunnerSettings _runnerSettings;

        public IRunnerSettingsBuilder SetOutputFolder(string outputBasePath)
        {
            _runnerSettings = new RunnerSettings(outputBasePath);
            return this;
        }

        public IRunnerSettingsBuilder PrependFilenames(string prependWith)
        {
            if (_runnerSettings == null) throw new Exception("Must start with SetOutputFolder");

            if (!string.IsNullOrWhiteSpace(prependWith))
                _runnerSettings.PrependFilenames(prependWith);
            return this;
        }

        public IRunnerSettingsBuilder SetResultsXmlFilename(string filename)
        {
            _runnerSettings.ResultsXmlFilename = filename;
            return this;
        }

        public IRunnerSettingsBuilder SetResultsHtmlReportFilename(string filename)
        {
            _runnerSettings.ResultsHtmlReportFilename = filename;
            return this;
        }

        public IRunnerSettingsBuilder SetResultsStatsFilename(string filename)
        {
            _runnerSettings.ResultsStatsFilename = filename;
            return this;
        }

        public IRunnerSettingsBuilder SetNamespace(string ns)
        {
            if (!string.IsNullOrEmpty(ns))
                _runnerSettings.Namespace = ns;

            return this;
        }

        public IRunnerSettingsBuilder RunUncategorizedTestFixturesParallel(bool runParallel = true)
        {
            _runnerSettings.RunUncategorizedTestFixturesParallel = runParallel;
            return this;
        }

        public IRunnerSettingsBuilder IncludeIgnoredFeatures(bool include)
        {
            _runnerSettings.IncludeIgnoredFeatures = include;
            return this;
        }

        public IRunnerSettingsBuilder RerunFailedCategories(bool rerun)
        {
            _runnerSettings.RerunFailedCategories = rerun;
            return this;
        }

        public IParallelizationSettingsBuilder ForParallelization()
        {
            return this;
        }

        public ITestRunActionSettingsBuilder ForTestRunAction()
        {
            return this;
        }

        public IRunnerSettingsBuilder UseTaskParallel()
        {
            _runnerSettings.ThreadingType = ParallelizationMethod.UseTaskParallel;
            return this;
        }

        public IRunnerSettingsBuilder UseDotNetThreadPool()
        {
            _runnerSettings.ThreadingType = ParallelizationMethod.UseDotNetThreadPool;
            return this;
        }

        public IRunnerSettingsBuilder UseExternalProcesses()
        {
            _runnerSettings.TestActionType = TestActionType.ExternalProcesses;
            return this;
        }

        public IRunnerSettingsBuilder UseInternalThreads()
        {
            _runnerSettings.TestActionType = TestActionType.InternalThreads;
            return this;
        }


        public RunnerSettings Build()
        {
            if (_runnerSettings == null) throw new Exception("Must start with SetOutputFolder");
            RunnerSettingsSingleton.Instance.Wrappee = _runnerSettings;
            return _runnerSettings;
        }
    }
}