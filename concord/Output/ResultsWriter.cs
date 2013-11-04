using System;
using System.Diagnostics;
using System.IO;
using concord.Configuration;
using concord.Logging;
using concord.Nunit;

namespace concord.Output
{
    public interface IResultsWriter
    {
        string MergeResultsProcess(string outputPath,
                                   string outputResultsXmlPath,
                                   string outputResultsReportPath);

        void GenerateHtmlReport(string outputPath, string outputResultsXmlPath, string outputResultsReportPath);
    }

    /// <summary>
    /// This is a front-end for the ResultMerger
    /// </summary>
    public class ResultsWriter : IResultsWriter
    {
        private readonly ILogger _logger;
        private readonly IResultMerger _resultMerger;

        public ResultsWriter(ILogger logger, IResultMerger resultMerger)
        {
            _logger = logger;
            _resultMerger = resultMerger;
        }

        public string MergeResultsProcess(string outputPath,
                                          string outputResultsXmlPath,
                                          string outputResultsReportPath)
        {
            CleanupPreviousFiles(outputResultsXmlPath, outputResultsReportPath);

            MergeResults(outputPath, outputResultsXmlPath);

            GenerateHtmlReport(outputPath, outputResultsXmlPath, outputResultsReportPath);

            var mergedContents = File.ReadAllText(outputResultsXmlPath);
            return mergedContents;
        }

        private static void CleanupPreviousFiles(string outputResultsXmlPath, string outputResultsReportPath)
        {
            File.Delete(outputResultsXmlPath);
            File.Delete(outputResultsReportPath);
        }

        private void MergeResults(string outputPath, string outputResultsXmlPath)
        {
            _logger.Log("Merged at" + outputPath);
            var mergedResults = _resultMerger.MergeResults(outputPath);
            _logger.Log("Merge results: " + mergedResults.XmlOutput);

            foreach (var file in Directory.GetFiles(outputPath, "*.xml", SearchOption.TopDirectoryOnly))
            {
                File.Delete(file);
            }
            _logger.Log("Written to: " + outputResultsXmlPath);

            File.WriteAllText(outputResultsXmlPath, mergedResults.XmlOutput);
            _logger.Log("Written: " + outputResultsXmlPath);

            if (mergedResults.Failures + mergedResults.Errors > 0)
            {
                Console.WriteLine();
                Console.WriteLine("ERRORS: Failures:{0}  Errors:{1}", mergedResults.Failures, mergedResults.Errors);
            }
            else
            {
                Console.WriteLine("Success, no errors!");
            }
        }

        public void GenerateHtmlReport(string outputPath, string outputResultsXmlPath, string outputResultsReportPath)
        {
            var args = string.Format(@"--fileset={0} --todir {1} --out {2}", outputResultsXmlPath, outputPath, outputResultsReportPath);
            _logger.Log("args: " + args);
            var processStartInfo = new ProcessStartInfo(Settings.Instance.NunitReportGeneratorPath, args)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
            };
            var processReport = new Process
            {
                StartInfo = processStartInfo
            };
            processReport.Start();
        }
    }
}