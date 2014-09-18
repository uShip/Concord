using System.Collections.Generic;

namespace concord.Output.Dto
{
    public class RunStatsCollection
    {
        public RunStatsCollection()
        {
            StatsVersion = -1;
            Records = new List<RunStats>();
        }

        public int StatsVersion { get; set; }
        public IEnumerable<RunStats> Records { get; set; }
    }
}