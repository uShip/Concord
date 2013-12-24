using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Core;
using NUnit.Core.Filters;
using concord.Configuration;
using concord.Output;
using concord.Parsers;
using concord.Wrappers;
using concord.Extensions;

namespace concord.Builders
{
    /// <summary>
    /// This runs the nunit commandlines as Process objects,
    ///   allowing us to control how many run at one time more easily
    /// This will use the same amount of RAM as the Batch file method would, if not limiting processes
    /// </summary>
    internal class ProcessRunner : IRunner
    {
        private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private readonly IResultsWriter _resultsWriter;
        private readonly IResultsParser _resultsParser;
        private readonly IProgressDisplay _progressDisplayBuilder;
        private readonly IResultsStatsWriter _resultsStatsWriter;
        private readonly IResultsOrderService _resultsOrderService;

        public ProcessRunner(
            IResultsWriter resultsWriter,
            IResultsParser resultsParser,
            IProgressDisplay progressDisplayBuilder,
            IResultsStatsWriter resultsStatsWriter,
            IResultsOrderService resultsOrderService)
        {
            _resultsWriter = resultsWriter;
            _resultsParser = resultsParser;
            _progressDisplayBuilder = progressDisplayBuilder;
            _resultsStatsWriter = resultsStatsWriter;
            _resultsOrderService = resultsOrderService;
        }

        private bool _configured = false;
        private string _assemblyLocation;
        private IEnumerable<string> _otherTestFixtures;
        private List<string> _categories;
        private List<string> _categoriesToRun;
        private RunnerSettings _runnerSettings;

        public void ConfigureRun(
            string assemblyLocation,
            IEnumerable<string> categories,
            IEnumerable<string> otherTestFixtures,
            IEnumerable<string> categoriesToRun,
            RunnerSettings runnerSettings)
        {
            if (_configured)
            {
                throw new InvalidOperationException("Need to at least wait until this run is complete..." + "\n"
                                                    + "Then we can talk, but for now, just create a new one");
            }

            _assemblyLocation = assemblyLocation;
            _otherTestFixtures = otherTestFixtures;
            _categories = categories.ToList();
            _categoriesToRun = categoriesToRun.ToList();
            _runnerSettings = runnerSettings;
            _configured = true;
        }

        public string GetRunResultsAsXml()
        {
            return GetRunResultsAsXml(-1);
        }

        public string GetRunResultsAsXml(int maxConcurrentRunners)
        {
            if (!_configured)
            {
                throw new InvalidOperationException("You must call ConfigureRun first");
            }

            //Keep a reference to standard out
            var stdOut = new TextWriterWrapper(Console.Out);
            var totalRuntime = new Stopwatch();
            totalRuntime.Start();


            var outputPath = _runnerSettings.OutputBasePath;
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);


            var shouldRunOther = _categoriesToRun.Count == 0
                                  || _categoriesToRun.Contains("all");

            //var categoryMessage = "Categories run will be: " + string.Join(", ", runnableCategories);
            //Debug.WriteLine(categoryMessage);
            //Console.WriteLine(categoryMessage);

            stdOut.WriteLine("Starting tests...");
            if (maxConcurrentRunners > 0) stdOut.WriteLine("   Running upto " + maxConcurrentRunners + " concurrently");
            stdOut.WriteLine("(ctrl-c and a few seconds to cancel, '{0}' means running, '{1}' means finished)",
                             _progressDisplayBuilder.ArrayValueToRunningStatusChar(ProgressState.Running),
                             _progressDisplayBuilder.ArrayValueToRunningStatusChar(ProgressState.Finished));

            
            var testFixturesToRun = new List<string>();
            if (shouldRunOther)
            {
                testFixturesToRun = new List<string>(_otherTestFixtures);
            }

            var runnableCategories = _categoriesToRun.Count > 0
                                         ? _categories.Intersect(_categoriesToRun).ToList()
                                         : _categories;
            int totalToRun = runnableCategories.Count();
            if (_runnerSettings.RunUncategorizedTestFixturesParallel)
            {
                totalToRun += testFixturesToRun.Count;
            }
            else if (shouldRunOther && _otherTestFixtures.Any())
            {
                totalToRun += 1;
            }

            stdOut.WriteLine();
            stdOut.WriteLine("Found {0} categories/fixtures to run", totalToRun);

            var testResults = new ConcurrentBag<RunStats>();
            bool cancelled = false;

            try
            {
                var runningTests = new ProgressStats(totalToRun);

                int indicatorPos = 0;
                var buildingDisplay = new object();
                var timer = new Timer(x =>
                    {
                        if (Console.IsOutputRedirected) return;
                        if (!Monitor.TryEnter(buildingDisplay)) return;

                        try
                        {
                            int windowWidth = Console.WindowWidth;

                            stdOut.Write("\r");
                            stdOut.Write(_progressDisplayBuilder.BuildProgressDisplay(windowWidth, runningTests,
                                                              ref indicatorPos,
                                                              _runnerSettings.DisplayFailureSymbolsInProgressDisplay));
                        }
                        catch (Exception exception)
                        {
                            stdOut.Write("display error...");
                            throw new ApplicationException("Unable to properly build progress display.", exception);
                        }
                        finally
                        {
                            Monitor.Exit(buildingDisplay);
                        }
                    }, null, 0, 250);

                if (Console.IsOutputRedirected)
                    timer.Change(Timeout.Infinite, Timeout.Infinite);

                //Setup ability to catch ctrl-c
                Console.CancelKeyPress += (sender, args) =>
                    {
                        timer.Change(Timeout.Infinite, Timeout.Infinite);

                        args.Cancel = true;
                        _cancelTokenSource.Cancel();

                        //stdOut.WriteLine();
                        //stdOut.WriteLine("CANCEL KEY PUSHED");

                        //Stop any running ones... maybe don't do this until pressed twice?
                        Process.GetProcesses()
                               .Where(p => p.ProcessName == "nunit-console"
                                           || p.ProcessName == "nunit-agent")
                               .Each(x => x.Kill());
                    };

                var startOrderInt = 0;
                var token = _cancelTokenSource.Token;

                var buildSortedAllActions = BuildSortedAllActions(testFixturesToRun, runnableCategories)
                    .ToArray();
                RunActionsOnThreads(maxConcurrentRunners, buildSortedAllActions, token, stdOut, runningTests, startOrderInt, totalRuntime, testResults);
                //Waiting for complete
                while (_threadCounter > 0)
                    Thread.Sleep(500);


                timer.Change(0, Timeout.Infinite);
                //Tacky way to fix line printing problem
                Thread.Sleep(100);
                stdOut.WriteLine();
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
                stdOut.WriteLine();
                stdOut.WriteLine("== Cancelled ==");
            }

            totalRuntime.Stop();
            stdOut.WriteLine("= Total runtime: " + TimeSpanFormat(totalRuntime.Elapsed));
            stdOut.WriteLine("Finished with tests, merging results");

            var SkippedTests = _categories.Except(testResults.Select(a => a.Name)).ToList();

            _resultsStatsWriter.OutputRunStats(totalRuntime.Elapsed, testResults, SkippedTests);
            _resultsOrderService.OutputRunOrder(testResults, SkippedTests);

            var outputResultsXmlPath = _runnerSettings.ResultsXmlFilepath;
            var outputResultsReportPath = _runnerSettings.ResultsHtmlReportFilepath;
            var xmlOutput = _resultsWriter.MergeResultsProcess(outputPath, outputResultsXmlPath, outputResultsReportPath);

            if (cancelled)
            {
                Environment.ExitCode = -9;
            }
            if (testResults.Any(x => x.ExitCode != 0))
            {
                stdOut.WriteLine("ERROR: Test process exited with error!");
                Environment.ExitCode = -1;
            }

            //Do we really need to return this? if so we should have the writing happen elsewhere...
            return xmlOutput;
        }

        private static void RunActionsOnThreads(int maxConcurrentRunners, IEnumerable<TestRunAction> buildSortedAllActions,
                                               CancellationToken token, TextWriterWrapper stdOut, ProgressStats runningTests,
                                               int startOrderInt, Stopwatch totalRuntime, ConcurrentBag<RunStats> testResults)
        {
            var sortedAllActions = buildSortedAllActions as TestRunAction[]
                                   ?? buildSortedAllActions.ToArray();

            if (maxConcurrentRunners > 0)
                ThreadPool.SetMaxThreads(maxConcurrentRunners, maxConcurrentRunners);

            for (var i = 0; i < sortedAllActions.Count(); i++)
            {
                token.ThrowIfCancellationRequested();
                CreateThread(sortedAllActions[i], token, stdOut, runningTests, startOrderInt, totalRuntime, testResults);
            }

            StopProcessing();
        }

        static int _threadCounter = 0;
        
        private static void CreateThread(TestRunAction action,
            CancellationToken token, TextWriterWrapper stdOut, ProgressStats runningTests,
                                               int startOrderInt, Stopwatch totalRuntime, ConcurrentBag<RunStats> testResults)
        {
            var parameters = new MethodParameters
                {
                    Action = action,
                    RunningTests = runningTests,
                    StartOrderInt = startOrderInt,
                    StdOut = stdOut,
                    TestResults = testResults,
                    Token = token,
                    TotalRuntime = totalRuntime
                };
            Interlocked.Increment(ref _threadCounter);
            //Shouldn't really use built-in thread pool for long-running processes...
            ThreadPool.QueueUserWorkItem(RunTest, parameters);
        }

        private static void StopProcessing()
        {
            while (_threadCounter > 0)
                Thread.Sleep(500);
        }

        private class MethodParameters
        {
            public TestRunAction Action { get; set; }
            public CancellationToken Token { get; set; }
            public TextWriterWrapper StdOut { get; set; }
            public ProgressStats RunningTests { get; set; }
            public int StartOrderInt  { get; set; }
            public Stopwatch TotalRuntime  { get; set; }
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
            

//            stdOut.WriteLine("Just started " + action.Name);
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
            if (exitCode != 0) //Go to TestFailure
                runningTests.IncrementIndex(action.Index);
            if (exitCode < 0) //Go to RunFailure
                runningTests.IncrementIndex(action.Index);

            Interlocked.Decrement(ref _threadCounter);
        }


        public IEnumerable<TestRunAction> BuildSortedAllActions(IEnumerable<string> testFixtures, IEnumerable<string> runnableCategories)
        {
            var actions = BuildAllActions(testFixtures, runnableCategories).ToList();

            var TargetRunOrder = _resultsOrderService.GetCategoriesInOrder();

            //Take ones in the order of TargetRunOrder, then append any others after that
            //  Ideally the Others would go first...
            var tempDebugging = TargetRunOrder
                .Select(name => actions.SingleOrDefault(x => x.Name == name))
                .Where(foundAction => foundAction != null)
                .Union(actions)
                .ToList();
            //            Console.WriteLine("actions are: " + string.Join(", ", actions.Select(x => x.Name)));
            //            Console.WriteLine("targets are: " + string.Join(", ", TargetRunOrder.Select(x => x)));
            //            Console.WriteLine("results are: " + string.Join(", ", tempDebugging.Select(x => x.Name)));

            return tempDebugging;
        }

        private IEnumerable<TestRunAction> BuildAllActions(IEnumerable<string> testFixtures, IEnumerable<string> runnableCategories)
        {
            int indexOffset = 0;
            //TestFixtures that do not have a category:
            if (_runnerSettings.RunUncategorizedTestFixturesParallel)
            {
                foreach (var fixture in testFixtures.Select((x, i) => new {Name = x, Index = i}))
                {
                    var x = fixture.Name;
                    yield return new TestRunAction
                        {
                            Name = "Fix-" + x,
                            Index = fixture.Index,
                            RunTests = () => BuildFilteredBlockingProcess(x)
                        };
                    ++indexOffset;
                }
            }
            else
            {
                //If any other features exist, run them all as one test run
                //  This excludes all categories we know about
                if (testFixtures.Any())
                {
                    var other = GetExcludeFitler(_categories.Concat(new[] { "Long" }).ToArray());
                    yield return new TestRunAction
                        {
                            Name = "all",
                            Index = 0,
                            RunTests = () => BuildFilteredBlockingProcess("all", other)
                        };
                    indexOffset = 1;
                }
            }

            //Categories:
            foreach (var cat in runnableCategories.Select((x, i) => new {Name = x, Index = i + indexOffset}))
            {
                var x = cat.Name;
                yield return new TestRunAction
                    {
                        Name = x,
                        Index = cat.Index,
                        RunTests = () => BuildFilteredBlockingProcess(x, GetIncludeFilter(x))
                    };
            }
        }

        private int BuildFilteredBlockingProcess(string category, ITestFilter filter)
        {
            var args = BuildParameterString(BuildOutputXmlPath(category), ToParameterString(filter));
            return BuildBlockingProcess(args);
        }

        private int BuildFilteredBlockingProcess(string testFixture)
        {
            var args = BuildParameterString(BuildOutputXmlPath("fixture-" + testFixture), SpecifyFixture(testFixture));
            return BuildBlockingProcess(args);
        }

        private int BuildBlockingProcess(string args)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo(Settings.Instance.NunitPath, args)
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                    };
                var testRun = new Process
                    {
                        StartInfo = processStartInfo,
                    };

                testRun.Start();
                testRun.PriorityClass = ProcessPriorityClass.BelowNormal;
                testRun.WaitForExit();
                return testRun.ExitCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR: " + ex.ToString());
                Console.Error.WriteLine("nunit path: " + Settings.Instance.NunitPath);
                throw;
            }
        }

        public string BuildParameterString(string outputXmlPath, string parameters)
        {
            return string.Format(@"{0} /xml:{1} {2}",
                                 parameters,
                                 outputXmlPath,
                                 _assemblyLocation);
        }

        private string BuildOutputXmlPath(string category)
        {
            return Path.Combine(_runnerSettings.OutputBasePath, string.Format("{0}.xml", category));
        }

        public string ToParameterString(ITestFilter filter)
        {
            var str = filter.ToString();
            if (str.StartsWith("not "))
            {
                return "/exclude:" + str.Substring("not ".Length);
            }
            return "/include:" + str;
        }

        /// <summary>
        /// Can be a class or namespace
        /// </summary>
        /// <param name="fixture"></param>
        /// <returns></returns>
        public string SpecifyFixture(string fixture)
        {
            return "/fixture:" + fixture;
        }

        public ITestFilter GetIncludeFilter(string includeCategory)
        {
            return new CategoryFilter(includeCategory);
        }

        public ITestFilter GetExcludeFitler(string[] excludeCategories)
        {
            return new NotFilter(new CategoryFilter(excludeCategories));
        }

        

        public static string TimeSpanFormat(TimeSpan ts)
        {
            return string.Format("{0} min{2} {1} secs", (int) ts.TotalMinutes, ts.Seconds, (int) ts.TotalMinutes == 1 ? "" : "s");
        }

        public class TestRunAction
        {
            public string Name { get; set; }
            public int Index { get; set; }
            public Func<int> RunTests { get; set; }
        }
    }
}