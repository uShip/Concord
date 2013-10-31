using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace concord.Output
{
    public interface IResultsStatsWriter
    {
        void OutputRunStats(string statsOutputFile, TimeSpan totalRuntime, IEnumerable<RunStats> runners, List<string> skippedTests);
    }

    public class ResultsStatsWriter : IResultsStatsWriter
    {
        public void OutputRunStats(string statsOutputFile, TimeSpan totalRuntime, IEnumerable<RunStats> runners, List<string> skippedTests)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<pre>");
            sb.AppendLine("Total Runtime: " + totalRuntime.ToString());
            foreach (var r in runners.OrderByDescending(t => t.RunTime))
            {
                sb.AppendLine(string.Format("{0} = {1} -- {2}  ExitCode:{3}", r.RunTime, r.FinishOrder, r.Name, r.ExitCode));
            }

            if (skippedTests.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Did not run:");
                foreach (var r in skippedTests)
                {
                    sb.AppendFormat("'{0}'", r);
                    sb.AppendLine();
                }
            }
            sb.AppendLine("</pre>");

            File.WriteAllText(statsOutputFile, sb.ToString());

            //var toOutput = new
            //{
            //    TotalRuntime = totalRuntime,
            //    Tests = runners.OrderByDescending(t => t.RunTime).ToList(),
            //    DidNotRun = skippedTests
            //};

            //toOutput.ToXml().Save(Path.Combine(outputPath, "RunStats.html"));
        }
    }

    public class RunStats
    {
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan RunTime { get; set; }
        public int FinishOrder { get; set; }

        public int ExitCode { get; set; }
    }
}