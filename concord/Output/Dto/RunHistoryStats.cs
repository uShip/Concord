using System;

namespace concord.Output.Dto
{
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