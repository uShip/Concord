using System;
using System.Globalization;

namespace concord.Output
{
    internal class ProgressDisplay : IProgressDisplay
    {
        public string BuildProgressDisplay(int width, ProgressStats runningTests, ref int indicatorPos, bool displayFailureSymbols)
        {
            int totalCount = runningTests.Count;
            int displayWidth = Math.Min(width - 4, totalCount);
            double displayRatio = (double)displayWidth / totalCount;

            Func<ProgressState, string> getProgressDisplay =
                x => new string(ArrayValueToRunningStatus(x), (int)(runningTests.GetProgressCount(x) * displayRatio));
            Func<ProgressState, int, string> getProgressDisplayLength =
                (x, i) => new string(ArrayValueToRunningStatus(x), (int)(i * displayRatio));

            int totalRunning = runningTests.GetProgressCount(ProgressState.Running);
            int totalFinished = displayFailureSymbols
                                    ? runningTests.GetProgressCount(ProgressState.Finished)
                                    : runningTests.GetCompletedCount();


            int finishedDisplayChars = (int)(totalFinished * displayRatio);
            int startedDisplayChars = (int)(totalRunning * displayRatio);
            int remainingDisplayChars = displayWidth - (int)(runningTests.GetCompletedCount() * displayRatio) - startedDisplayChars;

            return string.Format(displayFailureSymbols
                                     ? @"[{0}{1}{2}{3}{4}{5}]"
                                     : @"[{2}{3}{4}{5}]",
                                 getProgressDisplay(ProgressState.RunFailure),
                                 getProgressDisplay(ProgressState.TestFailure),
                                 getProgressDisplayLength(ProgressState.Finished, finishedDisplayChars),
                                 getProgressDisplayLength(ProgressState.Running, startedDisplayChars > 0 ? (startedDisplayChars - 1) : 0),
                                 startedDisplayChars > 0 ? WorkingIndicator[indicatorPos++ % WorkingIndicator.Length].ToString(CultureInfo.InvariantCulture) : "",
                                 getProgressDisplayLength(ProgressState.NotStarted, remainingDisplayChars));
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