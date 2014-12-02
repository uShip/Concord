using System;
using System.Collections.Generic;
using concord.Output.Dto;

namespace concord.Output
{
    public interface IResultsStatsWriter
    {
        [Obsolete("This is the old method... leaving for optional usage, when this becomes the default output, remove this")]
        void OutputRunStats(TimeSpan totalRuntime, IEnumerable<RunStats> runners, List<string> skippedTests);
        void OutputRunStats(TimeSpan totalRuntime, RunStatsCollection runStatsCollection);
    }
}