using System;
using System.Globalization;
using System.Linq;

namespace concord.Output
{
    internal class ProgressDisplay : IProgressDisplay
    {
        public string BuildProgressDisplay(int width, int[] runningTests, ref int indicatorPos)
        {
            int totalCount = runningTests.Length;
            int displayWidth = Math.Min(width - 4, totalCount);

            int totalRunning = runningTests.Count(x => x == Running);
            int totalFinished = runningTests.Count(x => x == Finished);


            int finishedDisplayChars = totalFinished * displayWidth / totalCount;
            int startedDisplayChars = totalRunning * displayWidth / totalCount;
            int remainingDisplayChars = displayWidth - finishedDisplayChars - startedDisplayChars;

            return string.Format(@"[{0}{1}{3}{2}]",
                                 new string(ArrayValueToRunningStatus(Finished), finishedDisplayChars),
                                 new string(ArrayValueToRunningStatus(Running), startedDisplayChars > 0 ? (startedDisplayChars - 1) : 0),
                                 new string(ArrayValueToRunningStatus(NotStarted), remainingDisplayChars),
                                 startedDisplayChars > 0 ? WorkingIndicator[indicatorPos++ % WorkingIndicator.Length].ToString(CultureInfo.InvariantCulture) : "");
        }

        //private string BuildProgressDisplay(int width, int[] runningTests)
        //{
        //    int displayWidth = Math.Min(width - 4, runningTests.Length);

        //    return string.Format(@"[{0}]",
        //                         new string(runningTests.Select(ArrayValueToRunningStatus).ToArray()));
        //}

        private readonly char[] WorkingIndicator = new[] {'|', '/', '─', '\\'};

        private const int NotStarted = 0;
        private const int Running = 1;
        private const int Finished = 2;

        public static char ArrayValueToRunningStatus(int value)
        {
            switch (value)
            {
                case NotStarted:
                    return '·';
                case Running:
                    return '*';
                case Finished:
                    return '=';
                default:
                    return 'e';
            }
        }

        public char ArrayValueToRunningStatusChar(int value)
        {
            return ArrayValueToRunningStatus(value);
        }
    }
}