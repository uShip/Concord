using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using concord.Builders.TestRunBuilders;
using concord.Output;
using concord.Output.Dto;
using concord.Wrappers;

namespace concord.Builders.ThreadingManagers
{
    interface IParallelManager
    {
        void RunActionsParallel(int maxConcurrentRunners, IEnumerable<TestRunAction> buildSortedAllActions, CancellationToken token, TextWriterWrapper stdOut, ProgressStats runningTests, Stopwatch totalRuntime, ConcurrentBag<RunStats> testResults);
    }
}