using System;
using System.Collections.Generic;
using concord.Output.Dto;

namespace concord.Output
{
    public class ResultsStatsTemplateWriter : IResultsStatsWriter
    {
        public void OutputRunStats(TimeSpan totalRuntime, IEnumerable<RunStats> runners, List<string> skippedTests)
        {
            
        }
    }
}