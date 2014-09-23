using System;
using System.Globalization;

namespace concord.Output
{
    internal class ProgressDisplay : IProgressDisplay
    {
        public string BuildProgressDisplay(int width, ProgressStats runningTests, ref int indicatorPos, bool displayFailureSymbols)
        {
            var totalCount = runningTests.Count;
            var displayWidth = Math.Min(width - 4, totalCount);
            var displayRatio = (double)displayWidth / totalCount;

            Func<ProgressState, string> getProgressDisplay =
                x =>
                {
                    var floatWidth = runningTests.GetProgressCount(x) * displayRatio;
                    //Round up decimals to 1
                    floatWidth = (floatWidth > 0 && floatWidth < 1) ? 1
                        : floatWidth;
                    return new string(ArrayValueToRunningStatus(x), (int)floatWidth);
                };
            Func<ProgressState, int, string> getProgressDisplayLength =
                (x, i) => new string(ArrayValueToRunningStatus(x), i);

            var totalRunning = runningTests.GetProgressCount(ProgressState.Running);
            var totalFinished = displayFailureSymbols
                                    ? runningTests.GetProgressCount(ProgressState.Finished)
                                    : runningTests.GetCompletedCount();
            var notStarted = runningTests.GetProgressCount(ProgressState.NotStarted);


            var finishedDisplayChars = (int)Math.Round(totalFinished * displayRatio);
            var startedDisplayChars = (int)Math.Ceiling(totalRunning * displayRatio);

            var progressBar =
                string.Format(displayFailureSymbols
                        ? @"[{0}{1}{2}{3}{4}{5}]"
                        : @"[{2}{3}{4}{5}]",
                    getProgressDisplay(ProgressState.RunFailure),
                    getProgressDisplay(ProgressState.TestFailure),
                    "{0}",
                    getProgressDisplayLength(ProgressState.Running, startedDisplayChars > 0 ? (startedDisplayChars - 1) : 0),
                    startedDisplayChars > 0 ? WorkingIndicator[indicatorPos++ % WorkingIndicator.Length].ToString(CultureInfo.InvariantCulture) : "",
                    "{1}");
            var intendedDisplayCharWidth = (finishedDisplayChars)
                                           + (progressBar.Length - "[{0}{1}]".Length);
            var remainingDisplayChars = displayWidth - intendedDisplayCharWidth;
            if (remainingDisplayChars < 0 || notStarted == 0)
            {
                //Trim down finished characters if there are too many characters.
                finishedDisplayChars += remainingDisplayChars;
                remainingDisplayChars = 0;
            }

            progressBar = string.Format(progressBar,
                getProgressDisplayLength(ProgressState.Finished, finishedDisplayChars),
                getProgressDisplayLength(ProgressState.NotStarted, remainingDisplayChars));

            return progressBar;
        }

        //private string BuildProgressDisplay(int width, int[] runningTests)
        //{
        //    int displayWidth = Math.Min(width - 4, runningTests.Length);

        //    return string.Format(@"[{0}]",
        //                         new string(runningTests.Select(ArrayValueToRunningStatus).ToArray()));
        //}

        private readonly char[] WorkingIndicator = new[] {'|', '/', '─', '\\'};

        public static char ArrayValueToRunningStatus(ProgressState value)
        {
            switch (value)
            {
                case ProgressState.NotStarted:
                    return '·';
                case ProgressState.Running:
                    return '*';
                case ProgressState.Finished:
                    return '=';
                case ProgressState.TestFailure:
                    return 'x';
                case ProgressState.RunFailure:
                    return '?';
                default:
                    return 'e';
            }
        }

        public char ArrayValueToRunningStatusChar(ProgressState value)
        {
            return ArrayValueToRunningStatus(value);
        }
    }
}