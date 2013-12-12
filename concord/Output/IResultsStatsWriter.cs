using System;
using System.Collections.Generic;

namespace concord.Output
{
    public interface IResultsStatsWriter
    {
        void OutputRunStats(TimeSpan totalRuntime, IEnumerable<RunStats> runners, List<string> skippedTests);
    }
}