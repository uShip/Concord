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
            string resultsStatsFilename = "RunStats.html",
            string resultsOrderDataFilename = "OrderData.json.html")
        {
            OutputBasePath = outputBasePath ?? Environment.CurrentDirectory;
            ResultsXmlFilename = resultsXmlFilename;
            ResultsHtmlReportFilename = resultsHtmlReportFilename;
            ResultsStatsFilename = resultsStatsFilename;
            ResultsOrderDataFilename = resultsOrderDataFilename;

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
            ResultsOrderDataFilename = prependWith + ResultsOrderDataFilename;
        }

        public string OutputBasePath { get; set; }
        public string ResultsXmlFilename { private get; set; }
        public string ResultsHtmlReportFilename { private get; set; }
        public string ResultsStatsFilename { private get; set; }
        public string ResultsOrderDataFilename { private get; set; }

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
        public string ResultsOrderDataFilepath
        {
            get
            {
                return Path.Combine(OutputBasePath, ResultsOrderDataFilename);
            }
        }

        public bool RunUncategorizedTestFixturesParallel { get; set; }

        public bool RerunFailedCategories { get; set; }
        public bool DisplayFailureSymbolsInProgressDisplay { get; set; }
    }
}