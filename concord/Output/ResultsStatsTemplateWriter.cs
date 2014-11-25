using System;
using System.Collections.Generic;
using concord.Configuration;
using concord.Output.Dto;
using concord.RazorTemplates.Models;

namespace concord.Output
{
    public class ResultsStatsTemplateWriter : IResultsStatsWriter
    {
        private readonly IRunnerSettings _settings;
        private readonly IResultsTemplateWriter _templateWriter;

        public ResultsStatsTemplateWriter(IRunnerSettings settings, IResultsTemplateWriter templateWriter)
        {
            _settings = settings;
            _templateWriter = templateWriter;
        }

        public void OutputRunStats(TimeSpan totalRuntime, IEnumerable<RunStats> runners, List<string> skippedTests)
        {
            //IEnumerable<LineData> rawData
            var model = new FancyResults
            {
                TotalRuntime = totalRuntime,
                Runners = runners,
                SkippedTests = skippedTests
            };

            _templateWriter.OutputResults(model, _settings.ResultsStatsFilepath);
        }
    }
}