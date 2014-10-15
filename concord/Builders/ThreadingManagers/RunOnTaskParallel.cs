using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using concord.Builders.TestRunBuilders;
using concord.Output;
using concord.Output.Dto;
using concord.Wrappers;

namespace concord.Builders.ThreadingManagers
{
    class RunOnTaskParallel : IParallelManager
    {
        public void RunActionsParallel(int maxConcurrentRunners, IEnumerable<TestRunAction> buildSortedAllActions, CancellationToken token, TextWriterWrapper stdOut, ProgressStats runningTests, Stopwatch totalRuntime, ConcurrentBag<RunStats> testResults)
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxConcurrentRunners > 0
                    ? maxConcurrentRunners
                    : -1,
                CancellationToken = token
            };

            var startOrderInt = 0;
            Parallel.ForEach(buildSortedAllActions,
                options,
                action =>
                {
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
                });
        }
    }
}