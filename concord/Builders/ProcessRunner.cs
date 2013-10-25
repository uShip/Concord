using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Core;
using NUnit.Core.Filters;
using concord.Configuration;
using concord.Logging;
using concord.Nunit;
using concord.Wrappers;

namespace concord.Builders
{
    /// <summary>
    /// This runs the nunit commandlines as Process objects,
    ///   allowing us to control how many run at one time more easily
    /// This will use the same amount of RAM as the Batch file method would, if not limiting processes
    /// </summary>
    internal class ProcessRunner : IRunner
    {
        private readonly string _assemblyLocation;
        private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private readonly List<string> _categories;
        private readonly List<string> _categoriesToRun;
        private readonly ILogger _logger;
        private readonly RunnerSettings _runnerSettings;

        public ProcessRunner(string assemblyLocation, IEnumerable<string> categories, IEnumerable<string> categoriesToRun,
                             ILogger logger, RunnerSettings runnerSettings)
        {
            _assemblyLocation = assemblyLocation;
            _categories = categories.ToList();
            _categoriesToRun = categoriesToRun.ToList();
            _logger = logger;
            _runnerSettings = runnerSettings;
        }

        public string GetRunResultsAsXml()
        {
            return GetRunResultsAsXml(-1);
        }

        public string GetRunResultsAsXml(int maxConcurrentRunners)
        {
            //Keep a reference to standard out
            var stdOut = new TextWriterWrapper(Console.Out);
            var totalRuntime = new Stopwatch();
            totalRuntime.Start();


            var outputPath = _runnerSettings.OutputBasePath;
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);


            bool shouldRunOther = _categoriesToRun.Count == 0
                                  || _categoriesToRun.Contains("all");

            //var categoryMessage = "Categories run will be: " + string.Join(", ", runnableCategories);
            //Debug.WriteLine(categoryMessage);
            //Console.WriteLine(categoryMessage);

            stdOut.WriteLine("Starting tests...");
            if (maxConcurrentRunners > 0) stdOut.WriteLine("   Running upto " + maxConcurrentRunners + " concurrently");
            stdOut.WriteLine("(ctrl-c and a few seconds to cancel, '{0}' means running, '{1}' means finished)",
                             ArrayValueToRunningStatus(1),
                             ArrayValueToRunningStatus(2));

            var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxConcurrentRunners > 0
                                                 ? maxConcurrentRunners
                                                 : -1,
                    CancellationToken = _cancelTokenSource.Token
                };
            var runnableCategories = _categoriesToRun.Count > 0
                                         ? _categories.Intersect(_categoriesToRun).ToList()
                                         : _categories;
            int totalToRun = runnableCategories.Count() + (shouldRunOther ? 1 : 0);

            stdOut.WriteLine();
            stdOut.WriteLine("Found {0} categories to run", totalToRun);

            var testResults = new ConcurrentBag<RunStats>();
            bool cancelled = false;

            try
            {
                var runningTests = new int[totalToRun];

                int indicatorPos = 0;
                var timer = new Timer(x =>
                    {
                        if (Console.IsOutputRedirected) return;

                        try
                        {
                            int windowWidth = Console.WindowWidth;

                            stdOut.Write("\r");
                            stdOut.Write(BuildProgressDisplay(windowWidth, runningTests,
                                                              ref indicatorPos));
                        }
                        catch (Exception exception)
                        {
                            throw new ApplicationException("Unable to properly build progress display.", exception);
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

                var token = options.CancellationToken;
                Parallel.ForEach(BuildAllActions(_categories, runnableCategories, shouldRunOther),
                                 options,
                                 action =>
                                     {
                                         token.ThrowIfCancellationRequested();

                                         Interlocked.Increment(ref runningTests[action.Index]);

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
                                                 FinishOrder = testResults.Count,
                                                 ExitCode = exitCode
                                             });

                                         Interlocked.Increment(ref runningTests[action.Index]);
                                     });

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

            OutputRunStats(outputPath, totalRuntime.Elapsed, testResults, SkippedTests);

            var xmlOutput = MergeResults(outputPath);

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

        public void OutputRunStats(string outputPath, TimeSpan totalRuntime, IEnumerable<RunStats> runners, List<string> skippedTests)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Total Runtime: " + totalRuntime.ToString());
            foreach (var r in runners.OrderByDescending(t => t.RunTime))
            {
                sb.AppendLine(string.Format("{0} = {1} -- {2}  ExitCode:{3}", r.RunTime, r.FinishOrder, r.Name, r.ExitCode));
            }

            if (skippedTests.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Did not run:");
                foreach (var r in skippedTests)
                {
                    sb.AppendFormat("'{0}'", r);
                    sb.AppendLine();
                }
            }

            File.WriteAllText(_runnerSettings.ResultsStatsFilepath, sb.ToString());

            //var toOutput = new
            //{
            //    TotalRuntime = totalRuntime,
            //    Tests = runners.OrderByDescending(t => t.RunTime).ToList(),
            //    DidNotRun = skippedTests
            //};

            //toOutput.ToXml().Save(Path.Combine(outputPath, "RunStats.html"));
        }

        private string MergeResults(string outputPath)
        {
            var outputResultsXmlPath = _runnerSettings.ResultsXmlFilepath;
            var outputResultsReportPath = _runnerSettings.ResultsHtmlReportFilepath;

            CleanupPreviousFiles(outputResultsXmlPath, outputResultsReportPath);

            MergeResults(outputPath, outputResultsXmlPath);

            GenerateHtmlReport(outputPath, outputResultsXmlPath, outputResultsReportPath);

            var mergedContents = File.ReadAllText(outputResultsXmlPath);
            return mergedContents;
        }

        private static void CleanupPreviousFiles(string outputResultsXmlPath, string outputResultsReportPath)
        {
            File.Delete(outputResultsXmlPath);
            File.Delete(outputResultsReportPath);
        }

        private void MergeResults(string outputPath, string outputResultsXmlPath)
        {
            var resultMerger = new ResultMerger();

            _logger.Log("Merged at" + outputPath);
            var mergedResults = resultMerger.MergeResults(outputPath);
            _logger.Log("Merge results: " + mergedResults.XmlOutput);

            foreach (var file in Directory.GetFiles(outputPath, "*.xml", SearchOption.TopDirectoryOnly))
            {
                File.Delete(file);
            }
            _logger.Log("Written to: " + outputResultsXmlPath);

            File.WriteAllText(outputResultsXmlPath, mergedResults.XmlOutput);
            _logger.Log("Written: " + outputResultsXmlPath);

            if (mergedResults.Failures + mergedResults.Errors > 0)
            {
                Console.WriteLine();
                Console.WriteLine("ERRORS: Failures:{0}  Errors:{1}", mergedResults.Failures, mergedResults.Errors);
            }
            else
            {
                Console.WriteLine("Success, no errors!");
            }
        }

        private void GenerateHtmlReport(string outputPath, string outputResultsXmlPath, string outputResultsReportPath)
        {
            var args = string.Format(@"--fileset={0} --todir {1} --out {2}", outputResultsXmlPath, outputPath, outputResultsReportPath);
            _logger.Log("args: " + args);
            var processStartInfo = new ProcessStartInfo(Settings.Instance.NunitReportGeneratorPath, args)
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                };
            var processReport = new Process
                {
                    StartInfo = processStartInfo
                };
            processReport.Start();
        }


        public IEnumerable<TestRunAction> BuildAllActions(List<string> categories, List<string> runnableCategories, bool shouldRunOther)
        {
            int indexOffset = 0;
            if (shouldRunOther)
            {
                var other = GetExcludeFitler(categories.Concat(new[] {"Long"}).ToArray());
                yield return new TestRunAction
                    {
                        Name = "all",
                        Index = 0,
                        RunTests = () => BuildBlockingProcess("all", other)
                    };
                indexOffset = 1;
            }

            foreach (var cat in runnableCategories.Select((x, i) => new {Name = x, Index = i + indexOffset}))
            {
                var x = cat.Name;
                yield return new TestRunAction
                    {
                        Name = x,
                        Index = cat.Index,
                        RunTests = () => BuildBlockingProcess(x, GetIncludefilter(x))
                    };
            }
        }

        public int BuildBlockingProcess(string category, ITestFilter filter)
        {
            try
            {
                var args = BuildParameterString(category, filter);

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

        public string BuildParameterString(string category, ITestFilter filter)
        {
            return string.Format(@"{0} /xml:{2} {1}",
                                 ToParameterString(filter),
                                 _assemblyLocation,
                                 Path.Combine(_runnerSettings.OutputBasePath, string.Format("{0}.xml", category)));
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

        public ITestFilter GetIncludefilter(string includeCategory)
        {
            if (includeCategory.StartsWith("_"))
                includeCategory = includeCategory.Substring(1);
            return new CategoryFilter(includeCategory);
        }

        public ITestFilter GetExcludeFitler(string[] excludeCategories)
        {
            return new NotFilter(new CategoryFilter(excludeCategories));
        }

        private string BuildProgressDisplay(int width, int[] runningTests, ref int indicatorPos)
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

        private readonly char[] WorkingIndicator = new[] { '|', '/', '─', '\\' };

        const int NotStarted = 0;
        const int Running = 1;
        const int Finished = 2;
        private static char ArrayValueToRunningStatus(int value)
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

        public static string TimeSpanFormat(TimeSpan ts)
        {
            return string.Format("{0} min{2} {1} secs", (int) ts.TotalMinutes, ts.Seconds, (int) ts.TotalMinutes == 1 ? "" : "s");
        }

        public class RunStats
        {
            public string Name { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public TimeSpan RunTime { get; set; }
            public int FinishOrder { get; set; }

            public int ExitCode { get; set; }
        }

        public class TestRunAction
        {
            public string Name { get; set; }
            public int Index { get; set; }
            public Func<int> RunTests { get; set; }
        }
    }
}