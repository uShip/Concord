using System;
using Newtonsoft.Json;

namespace concord.Output.Dto
{
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
}