using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using concord.Output;
using concord.Wrappers;
using TestRunAction = concord.Builders.ProcessRunner.TestRunAction;

namespace concord.Builders
{
    public class RunOnThreadPool
    {
        internal static void RunActionsOnThreads(int maxConcurrentRunners, IEnumerable<TestRunAction> buildSortedAllActions,
            CancellationToken token, TextWriterWrapper stdOut, ProgressStats runningTests,
            Stopwatch totalRuntime, ConcurrentBag<RunStats> testResults)
        {
            var sortedAllActions = buildSortedAllActions as TestRunAction[]
                                   ?? buildSortedAllActions.ToArray();

            if (maxConcurrentRunners <= 0)
            {
                maxConcurrentRunners = 12;
            }

            //Waiting for complete
            for (var i = 0; i < sortedAllActions.Count(); i++)
            {
                token.ThrowIfCancellationRequested();
                CreateThread(sortedAllActions[i], token, stdOut, runningTests, totalRuntime, testResults);

                //Stop queueing items once reach the limit... really should just always set this to the ThreadPool size...
                while (_threadCounter >= maxConcurrentRunners)
                    Thread.Sleep(500);
            }
            while (_threadCounter > 0)
                Thread.Sleep(500);
        }

        private static int _threadCounter = 0;
        private static int _startOrderInt = 0;

        private static void CreateThread(TestRunAction action,
            CancellationToken token, TextWriterWrapper stdOut, ProgressStats runningTests,
            Stopwatch totalRuntime, ConcurrentBag<RunStats> testResults)
        {
            var startOrder = Interlocked.Increment(ref _startOrderInt);
            var parameters = new MethodParameters
            {
                Action = action,
                RunningTests = runningTests,
                StartOrderInt = startOrder,
                StdOut = stdOut,
                TestResults = testResults,
                Token = token,
                TotalRuntime = totalRuntime
            };
            Interlocked.Increment(ref _threadCounter);
            //Shouldn't really use built-in thread pool for long-running processes...
            ThreadPool.QueueUserWorkItem(RunTest, parameters);
        }

        private class MethodParameters
        {
            public TestRunAction Action { get; set; }
            public CancellationToken Token { get; set; }
            public TextWriterWrapper StdOut { get; set; }
            public ProgressStats RunningTests { get; set; }
            public int StartOrderInt { get; set; }
            public Stopwatch TotalRuntime { get; set; }
            public ConcurrentBag<RunStats> TestResults { get; set; }
        }

        private static void RunTest(object parameters)
        {
            RunTest(parameters as MethodParameters);
        }

        private static void RunTest(MethodParameters mp)
        {
            var action = mp.Action;
            var token = mp.Token;
            var stdOut = mp.StdOut;
            var runningTests = mp.RunningTests;
            var startOrderInt = mp.StartOrderInt;
            var totalRuntime = mp.TotalRuntime;
            var testResults = mp.TestResults;

            //stdOut.Write(string.Format("\r> Starting: {0}   \n", action.Name));
            token.ThrowIfCancellationRequested();

            runningTests.IncrementIndex(action.Index);
            var startOrder = Interlocked.Increment(ref startOrderInt);
            var startTime = totalRuntime.Elapsed;
            var sw = new Stopwatch();
            sw.Start();
            var exitCode = action.RunTests();
            sw.Stop();
            testResults.Add(new RunStats
            {
                Name = action.Name,
                StartTime = startTime,
                RunTime = sw.Elapsed,
                EndTime = totalRuntime.Elapsed,
                StartOrder = startOrder,
                FinishOrder = testResults.Count,
                ExitCode = exitCode
            });
            //            stdOut.WriteLine("Just finished " + action.Name);
            runningTests.IncrementIndex(action.Index);
            if (exitCode != 0)
            {
                //Go to TestFailure
                runningTests.IncrementIndex(action.Index);
                if (!Console.IsOutputRedirected)
                    stdOut.Write("\r! Test failure: {0} ({1})   \n", action.Name, exitCode);
            }
            if (exitCode < 0)
            {
                //Go to RunFailure
                runningTests.IncrementIndex(action.Index);
            }

            Interlocked.Decrement(ref _threadCounter);
        }
    }
}