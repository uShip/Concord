using System;
using System.IO;

namespace concord.Configuration
{
    public class RunnerSettings : IRunnerSettings
    {
        public RunnerSettings(
            string outputBasePath,
            string resultsXmlFilename = "results.xml",
            string resultsHtmlReportFilename = "report.html",
            string resultsStatsFilename = "RunStats.html")
        {
            OutputBasePath = outputBasePath ?? Environment.CurrentDirectory;
            ResultsXmlFilename = resultsXmlFilename;
            ResultsHtmlReportFilename = resultsHtmlReportFilename;
            ResultsStatsFilename = resultsStatsFilename;

            //Defaults:
            DisplayFailureSymbolsInProgressDisplay = true;
        }

        public RunnerSettings(string outputBasePath, string prependFilenames)
            : this(outputBasePath)
        {
            OutputBasePath = outputBasePath;

            PrependFilenames(prependFilenames);
        }

        internal void PrependFilenames(string prependWith)
        {
            ResultsXmlFilename = prependWith + ResultsXmlFilename;
            ResultsHtmlReportFilename = prependWith + ResultsHtmlReportFilename;
            ResultsStatsFilename = prependWith + ResultsStatsFilename;
        }

        public string OutputBasePath { get; set; }
        public string ResultsXmlFilename { private get; set; }
        public string ResultsHtmlReportFilename { private get; set; }
        public string ResultsStatsFilename { private get; set; }

        public string Namespace { get; set; }

        public string ResultsXmlFilepath
        {
            get
            {
                return Path.Combine(OutputBasePath, ResultsXmlFilename);
            }
        }
        public string ResultsHtmlReportFilepath
        {
            get
            {
                return Path.Combine(OutputBasePath, ResultsHtmlReportFilename);
            }
        }
        public string ResultsStatsFilepath
        {
            get
            {
                return Path.Combine(OutputBasePath, ResultsStatsFilename);
            }
        }

        public bool RunUncategorizedTestFixturesParallel { get; set; }

        public bool RerunFailedCategories { get; set; }
        public bool DisplayFailureSymbolsInProgressDisplay { get; set; }
    }
}