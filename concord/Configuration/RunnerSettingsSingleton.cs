using System;

namespace concord.Configuration
{
    class RunnerSettingsSingleton : IRunnerSettings
    {
        private readonly static Lazy<RunnerSettingsSingleton> _lazyInstance = new Lazy<RunnerSettingsSingleton>(() => new RunnerSettingsSingleton());
        private IRunnerSettings _wrappee;
        public static RunnerSettingsSingleton Instance { get { return _lazyInstance.Value; } }

        public IRunnerSettings Wrappee { set { _wrappee = value; } }

        private RunnerSettingsSingleton()
        {
            
        }

        public string OutputBasePath
        {
            get { return _wrappee.OutputBasePath; }
            set { _wrappee.OutputBasePath = value; }
        }

        public string ResultsXmlFilename
        {
            set { _wrappee.ResultsXmlFilename = value; }
        }

        public string ResultsHtmlReportFilename
        {
            set { _wrappee.ResultsHtmlReportFilename = value; }
        }

        public string ResultsStatsFilename
        {
            set { _wrappee.ResultsStatsFilename = value; }
        }

        public string Namespace
        {
            get { return _wrappee.Namespace; }
            set { _wrappee.Namespace = value; }
        }

        public string ResultsXmlFilepath
        {
            get { return _wrappee.ResultsXmlFilepath; }
        }

        public string ResultsHtmlReportFilepath
        {
            get { return _wrappee.ResultsHtmlReportFilepath; }
        }

        public string ResultsStatsFilepath
        {
            get { return _wrappee.ResultsStatsFilepath; }
        }

        public string ResultsOrderDataFilepath
        {
            get { return _wrappee.ResultsOrderDataFilepath; }
        }

        public bool RunUncategorizedTestFixturesParallel
        {
            get { return _wrappee.RunUncategorizedTestFixturesParallel; }
            set { _wrappee.RunUncategorizedTestFixturesParallel = value; }
        }

        public bool IncludeIgnoredFeaturesInStats
        {
            get { return _wrappee.IncludeIgnoredFeaturesInStats; }
            set { _wrappee.IncludeIgnoredFeaturesInStats = value; }
        }

        public bool RerunFailedCategories
        {
            get { return _wrappee.RerunFailedCategories; }
            set { _wrappee.RerunFailedCategories = value; }
        }

        public bool DisplayFailureSymbolsInProgressDisplay
        {
            get { return _wrappee.DisplayFailureSymbolsInProgressDisplay; }
            set { _wrappee.DisplayFailureSymbolsInProgressDisplay = value; }
        }

        public ThreadingType ThreadingType
        {
            get { return _wrappee.ThreadingType; }
            set { _wrappee.ThreadingType = value; }
        }
    }
}