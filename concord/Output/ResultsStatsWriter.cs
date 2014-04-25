using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using concord.Configuration;

namespace concord.Output
{
    public class ResultsStatsWriter : IResultsStatsWriter
    {
        private readonly IRunnerSettings _settings;
        private readonly IHtmlGanttChart _ganttChartBuilder;

        public ResultsStatsWriter(IRunnerSettings settings, IHtmlGanttChart ganttChartBuilder)
        {
            _settings = settings;
            _ganttChartBuilder = ganttChartBuilder;
        }

        public void OutputRunStats(TimeSpan totalRuntime, IEnumerable<RunStats> runners, List<string> skippedTests)
        {
            var sb = new StringBuilder();

            sb.AppendLine(_ganttChartBuilder.GenerateGanttChart(runners));

            sb.AppendLine("<pre>");
            sb.AppendLine("Total Runtime: " + totalRuntime.ToString());
            foreach (var r in runners.OrderByDescending(t => t.RunTime))
            {
                AppendTestFinishedLine(sb, r);
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

            File.WriteAllText(_settings.ResultsStatsFilepath, sb.ToString());
        }

        //TODO test that this line can be processed by ResultsParser
        internal void AppendTestFinishedLine(StringBuilder sb, RunStats r)
        {
            sb.AppendLine(string.Format("{0} = {1}-{2} -- {3}  ExitCode:{4}", r.RunTime, r.StartOrder, r.FinishOrder, r.Name, r.ExitCode));
        }
    }

    public class RunStats : RunHistoryStats
    {
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan RunTime { get; set; }
        public int FinishOrder { get; set; }
        public int StartOrder { get; set; }
        public int ExitCode { get; set; }

        public int TestRunId { get; set; }
    }

    public abstract class RunHistoryStats
    {
        public void CopyHistoryStatsFrom(RunHistoryStats history)
        {
            DatapointsInAverage = history.DatapointsInAverage;
            AverageTime = history.AverageTime;
            FailureCount = history.FailureCount;
        }

        public void AddDatapoint(TimeSpan runLength)
        {
            var diff = runLength.TotalMilliseconds - AverageTime.TotalMilliseconds;
            var diffAvg = diff / ++DatapointsInAverage;
            AverageTime = AverageTime.Add(TimeSpan.FromMilliseconds(diffAvg));
        }

        public void IncrementFailedCount()
        {
            FailureCount++;
        }

        public int DatapointsInAverage { get; set; }
        public TimeSpan AverageTime { get; set; }

        public int FailureCount { get; set; }
    }
}