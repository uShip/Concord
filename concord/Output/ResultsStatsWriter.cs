using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using concord.Output.Dto;
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
}