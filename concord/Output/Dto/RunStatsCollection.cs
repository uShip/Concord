using System.Collections.Generic;

namespace concord.Output.Dto
{
    public class RunStatsCollection
    {
        private RunStatsCollection()
        {
            StatsVersion = -1;
            Records = new List<RunStats>();
        }

        public static RunStatsCollection BuildCurrent(IEnumerable<RunStats> stats)
        {
            return new RunStatsCollection
            {
                StatsVersion = 2,
                Records = stats
            };
        }

        public static RunStatsCollection BuildVersion0(IEnumerable<RunStats> stats)
        {
            return new RunStatsCollection
            {
                StatsVersion = 0,
                Records = stats
            };
        }

        public int StatsVersion { get; set; }
        public IEnumerable<RunStats> Records { get; set; }
    }
}