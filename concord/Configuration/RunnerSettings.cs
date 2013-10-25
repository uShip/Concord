using System;
using System.IO;

namespace concord.Configuration
{
    public class RunnerSettings
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
        }

        public RunnerSettings(string outputBasePath, string prependFiles)
            : this(outputBasePath)
        {
            OutputBasePath = outputBasePath;

            ResultsXmlFilename = prependFiles + ResultsXmlFilename;
            ResultsHtmlReportFilename = prependFiles + ResultsHtmlReportFilename;
            ResultsStatsFilename = prependFiles + ResultsStatsFilename;
        }

        public string OutputBasePath { get; set; }
        public string ResultsXmlFilename { private get; set; }
        public string ResultsHtmlReportFilename { private get; set; }
        public string ResultsStatsFilename { private get; set; }

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
    }
}