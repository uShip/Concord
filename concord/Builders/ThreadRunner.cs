using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Util;
using concord.Configuration;
using concord.Extensions;
using concord.Logging;
using concord.Nunit;
using concord.Wrappers;

namespace concord.Builders
{
    /// <summary>
    /// This is running it in Threads using the TestRunner class from nunit
    /// NOTE I have not gotten this to work reliably!!!!!
    ///  --Config not found somtimes, had to copy it to the NunitCategoryRunner.exe.config location
    ///  --Problems with them running at the same time...
    /// If we can get this to work, it should use significantly less RAM than running the processes
    /// </summary>
    [Obsolete("Not working, nor finished... issues with config files getting loaded")]
    internal class ThreadRunner : IRunner
    {
        private readonly string _assemblyLocation;
        private readonly List<Type> _featureTypes;
        private readonly ILogger _logger;
        private readonly string _outputPath;

        private readonly bool _outputRunStats = false;

        public ThreadRunner(string assemblyLocation, IEnumerable<Type> featureTypes,
                            ILogger logger, string outputPath)
        {
            _assemblyLocation = assemblyLocation;
            _featureTypes = featureTypes.ToList();
            _logger = logger;
            _outputPath = outputPath;
        }

        public string GetRunResultsAsXml()
        {
            return GetRunResultsAsXml(100);
        }

        public string GetRunResultsAsXml(int maxConcurrentRunners)
        {
            //Keep a reference to standard out
            var stdOut = new TextWriterWrapper(Console.Out);
            var totalRuntime = new Stopwatch();
            totalRuntime.Start();


            var outputPath = _outputPath;
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);


            var categories = _featureTypes.GetCategories().ToList();
            var categoryMessage = "Categories run will be: " + string.Join(", ", categories);
            Debug.WriteLine(categoryMessage);
            Console.WriteLine(categoryMessage);

            var runners = new List<CategoryRunner>();

            using (var countdownEvent = new CountdownEvent(1))
            {
                int totalCount = categories.Count + 1;

                //NOTE: Ideally we want to start something like the top (concurrentRunners/2) longest running process first

                //Run all other tests
                var other = GetExcludeFitler(categories.ToArray());
                var anyOthers = new CategoryRunner(_assemblyLocation, other, "all", _outputPath, countdownEvent);
                runners.Add(anyOthers);
                anyOthers.RunTesterAsync();

                foreach (var cat in categories)
                {
                    var filter = GetIncludefilter(cat);
                    var runner = new CategoryRunner(_assemblyLocation, filter, cat, _outputPath, countdownEvent);
                    runners.Add(runner);
                    runner.RunTesterAsync();

                    while (maxConcurrentRunners > 0 && countdownEvent.CurrentCount > maxConcurrentRunners)
                    {
                        stdOut.WriteLine("Reached limit: " + (countdownEvent.CurrentCount - 1)
                                         + "  Runners created: " + runners.Count + " / " + totalCount);
                        Thread.Sleep(2000);
                    }
                }

                //Wait for all runners to complete, outputting status
                countdownEvent.Signal();
                while (countdownEvent.CurrentCount > 0)
                {
                    stdOut.WriteLine("Running: " + countdownEvent.CurrentCount
                                     + " / " + totalCount);
                    countdownEvent.Wait(2000);
                }
            }

            totalRuntime.Stop();
            if (_outputRunStats) OutputRunStats(outputPath, totalRuntime.Elapsed, runners);

            stdOut.WriteLine("Finished with tests, merging");

            return MergeResults(outputPath);
        }

        public void OutputRunStats(string outputPath, TimeSpan totalRuntime, IEnumerable<CategoryRunner> runners)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Total Runtime: " + totalRuntime.ToString());
            foreach (var r in runners.OrderByDescending(t => t.RunningTime))
            {
                sb.AppendLine(r.RunningTime + " -- " + r.OutputName);
            }

            File.WriteAllText(Path.Combine(outputPath, "RunStats.txt"), sb.ToString());
        }

        private string MergeResults(string outputPath)
        {
            var resultMerger = new ResultMerger();
            var outputResultsXmlPath = Path.Combine(outputPath, "results.xml");
            var outputResultsReportPath = Path.Combine(outputPath, "report.html");
            File.Delete(outputResultsXmlPath);
            File.Delete(outputResultsReportPath);
            _logger.Log("Merged at" + outputPath);
            var mergedResults = resultMerger.MergeResults(outputPath);
            _logger.Log("Merge results: " + mergedResults.XmlOutput);
            foreach (var file in Directory.GetFiles(outputPath, "*.xml", SearchOption.TopDirectoryOnly))
            {
                File.Delete(file);
            }
            _logger.Log("Written to: " + outputResultsXmlPath);
            var args = string.Format(@"--fileset={0} --todir {1} --out {2}", outputResultsXmlPath, outputPath, outputResultsReportPath);
            _logger.Log("args: " + args);
            File.WriteAllText(outputResultsXmlPath, mergedResults.XmlOutput);
            _logger.Log("Written: " + outputResultsXmlPath);
            var processReport = new Process
                {
                    StartInfo = new ProcessStartInfo(Settings.Instance.NunitReportGeneratorPath, args)
                };
            processReport.Start();
            var mergedContents = File.ReadAllText(outputResultsXmlPath);
            return mergedContents;
        }

        public ITestFilter GetIncludefilter(string includeCategory)
        {
            return new CategoryFilter(includeCategory);
        }

        public ITestFilter GetExcludeFitler(string[] excludeCategories)
        {
            return new NotFilter(new CategoryFilter(excludeCategories));
        }

        public class CategoryRunner : EventListener
        {
            private readonly ITestFilter Filter;
            public readonly string OutputName;
            private readonly string _assemblyLocation;
            private readonly string _outputPath;
            private readonly Stopwatch _timer = new Stopwatch();

            private readonly CountdownEvent countdownEventRef;

            public CategoryRunner(string assemblyLocation, ITestFilter filter, string outputName, string outputPath, CountdownEvent countdown)
            {
                _assemblyLocation = assemblyLocation;
                Filter = filter;
                OutputName = outputName;
                _outputPath = outputPath;

                countdownEventRef = countdown;
                MarkAsInitalized();
            }

            public bool FinishedRunning { private set; get; }
            public bool IsRunning { private set; get; }

            public TimeSpan RunningTime
            {
                get { return _timer.Elapsed; }
            }


            public void RunTesterAsync()
            {
                var tr = GetTestRunner();

                tr.BeginRun(this, Filter, false, LoggingThreshold.Off);
            }

            private void MarkAsInitalized()
            {
                FinishedRunning = false;
                countdownEventRef.AddCount();
            }

            private void MarkAsRunning()
            {
                IsRunning = true;
                _timer.Start();
            }

            private void MarkAsFinished()
            {
                _timer.Stop();
                FinishedRunning = true;
                IsRunning = false;
                countdownEventRef.Signal();
            }

            public TestRunner GetTestRunner()
            {
                //Doesn't help...
                //if (!CoreExtensions.Host.Initialized)
                //    CoreExtensions.Host.InitializeService();

                var pack = new TestPackage(_assemblyLocation);
                pack.ConfigurationFile = _assemblyLocation + ".config";
                var nu = new RemoteTestRunner();
                //var nu = new SimpleTestRunner(Thread.CurrentThread.ManagedThreadId);
                //var nu = new ThreadedTestRunner(new RemoteTestRunner());
                if (!nu.Load(pack))
                {
                    throw new ApplicationException("what... very rude stuff happened");
                }
                return nu;
            }


            #region EventListener methods

            public void RunFinished(TestResult result)
            {
                try
                {
                    var cleanedName = OutputName.Replace("?", "");
                    var writer = new XmlResultWriter(Path.Combine(_outputPath, string.Format("{0}.xml", cleanedName)));
                    writer.SaveTestResult(result);
                }
                finally
                {
                    //Finished
                    MarkAsFinished();
                }
            }

            public void RunStarted(string name, int testCount)
            {
                MarkAsRunning();
            }

            public void RunFinished(Exception exception)
            {
                MarkAsFinished();
                throw new NotImplementedException("RunFinished");
            }

            public void UnhandledException(Exception exception)
            {
                MarkAsFinished();
                throw new NotImplementedException("NotImplementedException");
            }


            public void TestStarted(TestName testName)
            {
            }

            public void TestFinished(TestResult result)
            { }

            public void SuiteStarted(TestName testName)
            { }

            public void SuiteFinished(TestResult result)
            { }

            public void TestOutput(TestOutput testOutput)
            { }

            #endregion EventListener methods
        }
    }
}