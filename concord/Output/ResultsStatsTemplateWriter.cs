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

        [Obsolete("This is the old method... leaving for optional usage, when this becomes the default output, remove this")]
        public void OutputRunStats(TimeSpan totalRuntime, IEnumerable<RunStats> runners, List<string> skippedTests)
        {
            //Do nothing, this is obsolete
        }

        public void OutputRunStats(TimeSpan totalRuntime, RunStatsCollection runStatsCollection)
        {
            //TODO handle null history
            if (runStatsCollection == null)
                throw new NotImplementedException("TODO Need to handle null history at sometime...");

            var model = new FancyResults
            {
                TotalRuntime = totalRuntime,
                Runners = runStatsCollection.Records
            };

            _templateWriter.OutputResults(model, _settings.ResultsStatsFilepath);
        }
    }
}