using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
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

        [JsonIgnore]
        public bool IsCurrentRun
        {
            get { return TestRunId == 0; }
        }
        [JsonIgnore]
        public bool IsSuccess
        {
            get { return ExitCode == 0; }
        }
    }

    public abstract class RunHistoryStats
    {
        public void CopyHistoryStatsFrom(RunHistoryStats history)
        {
            DatapointsInAverage = history.DatapointsInAverage;

            AverageTime = history.AverageTime;
            WeightedAverageTime = history.WeightedAverageTime;
            FailedAverageTime = history.FailedAverageTime;

            FailureCount = history.FailureCount;
        }

        public void AddDatapoint(TimeSpan runLength, bool isSuccess)
        {
            //Increment counts
            if (!isSuccess) FailureCount++;
            DatapointsInAverage++;

            //Only including success runtime in AverageTimes
            if (isSuccess)
            {
                var successDatapoints = DatapointsInAverage - FailureCount;
                successDatapoints = Math.Max(successDatapoints, 1);

                SetAverageTime(runLength, successDatapoints);

                SetWeightedAverageTime(runLength, successDatapoints);
            }
            else
            {
                SetFailedAverage(runLength);
            }
        }

        private void SetAverageTime(TimeSpan runLength, int successDatapoints)
        {
            var diff = runLength.TotalMilliseconds - AverageTime.TotalMilliseconds;
            var diffAvg = diff / successDatapoints;
            AverageTime = AverageTime.Add(TimeSpan.FromMilliseconds(diffAvg));
        }

        private void SetWeightedAverageTime(TimeSpan runLength, int successDatapoints)
        {
            //For weighted average, square the diff and add that
            //  Unless its just started running... don't want 0 + 20*20 when its 20
            var weightedDiff = runLength.TotalMilliseconds - WeightedAverageTime.TotalMilliseconds;
            var weightedDiffAvg = weightedDiff / successDatapoints;
            WeightedAverageTime = WeightedAverageTime.Add(successDatapoints > 5
                ? TimeSpan.FromMilliseconds(weightedDiffAvg * weightedDiffAvg)
                : TimeSpan.FromMilliseconds(weightedDiffAvg));
        }

        private void SetFailedAverage(TimeSpan runLength)
        {
            var failedDiff = runLength.TotalMilliseconds - FailedAverageTime.TotalMilliseconds;
            var failedDiffAvg = failedDiff / FailureCount;
            FailedAverageTime = FailedAverageTime.Add(TimeSpan.FromMilliseconds(failedDiffAvg));
        }

        public int DatapointsInAverage { get; set; }
        public int FailureCount { get; set; }

        public TimeSpan AverageTime { get; set; }
        /// <summary>
        /// This will react to changes in test runtime MUCH faster after large amounts of data is collected
        /// </summary>
        public TimeSpan WeightedAverageTime { get; set; }
        public TimeSpan FailedAverageTime { get; set; }
    }
}