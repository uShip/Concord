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
            //Work in seconds space since squaring 1000 miliseconds == 16 minutes...
            var weightedDiff = runLength.TotalSeconds - WeightedAverageTime.TotalSeconds;
            var weightedDiffAvg = weightedDiff / successDatapoints;

            //TODO maybe instead of squaring it, just limit the maximum number of successDatapoints???
            //  So just consider the existing weighted average to represent 5 data points only
            var weightedAdjustment = Math.Abs(weightedDiffAvg) * weightedDiffAvg;

            var newWeightedAverage = WeightedAverageTime.Add(successDatapoints > 5
                ? TimeSpan.FromSeconds(weightedAdjustment)
                : TimeSpan.FromSeconds(weightedDiffAvg));

            if (newWeightedAverage < TimeSpan.Zero)
            {
                //Whoops went negative, thats wrong
                Console.WriteLine("Went into negative time... that seems wrong... GOING TO ONLY HALF IT");
                newWeightedAverage = TimeSpan.FromMilliseconds(WeightedAverageTime.TotalMilliseconds / 2);
            }
            var currentValueSquared = Math.Max(30, 2 * WeightedAverageTime.TotalSeconds);
            if (newWeightedAverage > TimeSpan.FromSeconds(currentValueSquared))
            {
                //Limit it to at most doubling
                Console.WriteLine("Went up by more than double... that seems wrong... GOING TO ONLY 2X IT");
                newWeightedAverage = WeightedAverageTime.Add(WeightedAverageTime);
            }
            WeightedAverageTime = newWeightedAverage;
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