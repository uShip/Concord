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
using concord.Output;
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
        private readonly IResultsWriter _resultsWriter;
        //private readonly IProgressDisplay _progressDisplayBuilder;
        //private readonly IResultsStatsWriter _resultsStatsWriter;
        //private readonly IResultsOrderService _resultsOrderService;

        private readonly bool _outputRunStats = false;

        public ThreadRunner(
            IResultsWriter resultsWriter,
            IProgressDisplay progressDisplayBuilder,
            IResultsStatsWriter resultsStatsWriter,
            IResultsOrderService resultsOrderService)
        {
            _resultsWriter = resultsWriter;
//            _progressDisplayBuilder = progressDisplayBuilder;
//            _resultsStatsWriter = resultsStatsWriter;
//            _resultsOrderService = resultsOrderService;
        }

        private bool _configured = false;
        private string _assemblyLocation;
        private IEnumerable<string> _otherTestFixtures;
        private List<string> _categories;
        private List<string> _categoriesToRun;
        private IRunnerSettings _runnerSettings;

        public void ConfigureRun(
            string assemblyLocation,
            IEnumerable<string> categories,
            IEnumerable<string> otherTestFixtures,
            IEnumerable<string> categoriesToRun,
            IRunnerSettings runnerSettings)
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
            return GetRunResultsAsXml(100);
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

            var testFixturesToRun = new List<string>();
            if (shouldRunOther)
            {
                testFixturesToRun = new List<string>(_otherTestFixtures);
            }

            var runnableCategories = _categoriesToRun.Count > 0
                                         ? _categories.Intersect(_categoriesToRun).ToList()
                                         : _categories;
            int totalToRun = runnableCategories.Count();

            var runners = new List<CategoryRunner>();

            using (var countdownEvent = new CountdownEvent(1))
            {
//////                //Run all other tests
//////                var other = GetExcludeFitler(runnableCategories.ToArray());
//////                var anyOthers = new CategoryRunner(_assemblyLocation, other, "all", outputPath, countdownEvent);
//////                runners.Add(anyOthers);
//////                anyOthers.RunTesterAsync();

                foreach (var cat in runnableCategories)
                {
                    var filter = GetIncludefilter(cat);
                    var runner = new CategoryRunner(_assemblyLocation, filter, cat, outputPath, countdownEvent);
                    runners.Add(runner);
                    runner.RunTesterAsync();

                    while (maxConcurrentRunners > 0 && countdownEvent.CurrentCount > maxConcurrentRunners)
                    {
                        stdOut.WriteLine("Reached limit: " + (countdownEvent.CurrentCount - 1)
                                         + "  Runners created: " + runners.Count + " / " + totalToRun);
                        Thread.Sleep(2000);
                    }
                }

                //Wait for all runners to complete, outputting status
                countdownEvent.Signal();
                while (countdownEvent.CurrentCount > 0)
                {
                    stdOut.WriteLine("Running: " + countdownEvent.CurrentCount
                                     + " / " + totalToRun);
                    countdownEvent.Wait(2000);
                }
            }

            totalRuntime.Stop();
            if (_outputRunStats) OutputRunStats(outputPath, totalRuntime.Elapsed, runners);

            stdOut.WriteLine("Finished with tests, merging");

            var outputResultsXmlPath = _runnerSettings.ResultsXmlFilepath;
            var outputResultsReportPath = _runnerSettings.ResultsHtmlReportFilepath;
            var xmlOutput = _resultsWriter.MergeResultsProcess(outputPath, outputResultsXmlPath, outputResultsReportPath);
            return xmlOutput;
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

        public ITestFilter GetIncludefilter(string includeCategory)
        {
            return new CategoryFilter(includeCategory);
        }

        public ITestFilter GetExcludeFitler(string[] excludeCategories)
        {
            return new NotFilter(new CategoryFilter(excludeCategories));
        }

        public class CategoryRunner : EventListener, IDisposable
        {
            private readonly ITestFilter Filter;
            public readonly string OutputName;
            private readonly string _assemblyLocation;
            private readonly string _outputPath;
            private readonly Stopwatch _timer = new Stopwatch();

            private readonly CountdownEvent countdownEventRef;
            private TestRunner _testRunner;

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
                _testRunner = GetTestRunner();

                _testRunner.BeginRun(this, Filter, false, LoggingThreshold.Off);
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
                _testRunner.Dispose();
                _timer.Stop();
                FinishedRunning = true;
                IsRunning = false;
                countdownEventRef.Signal();
            }

            public void Dispose()
            {
                if (_testRunner != null)
                    _testRunner.Dispose();
            }

            public TestRunner GetTestRunner()
            {
                if (!CoreExtensions.Host.Initialized)
                {
                    InitalizeNUnits();
                }

                var pack = new TestPackage(_assemblyLocation);
                //TODO next two lines, really needed???
                pack.PrivateBinPath = Path.GetDirectoryName(_assemblyLocation);
                //pack.BasePath = Path.GetDirectoryName(_assemblyLocation);
                //pack.ConfigurationFile = _assemblyLocation + ".config";
                var nu = new MultipleTestDomainRunner();
                //var nu = new SimpleTestRunner(Thread.CurrentThread.ManagedThreadId);
                //var nu = new ThreadedTestRunner(new RemoteTestRunner());
                if (!nu.Load(pack))
                {
                    throw new ApplicationException("what... very rude stuff happened");
                }
                return nu;
            }

            private void InitalizeNUnits()
            {
                Console.WriteLine("INITALIZING THIS STUFF");
                // Add Standard Services to ServiceManager
                //			ServiceManager.Services.AddService( settingsService );
                ServiceManager.Services.AddService(new DomainManager());
                //ServiceManager.Services.AddService( new RecentFilesService() );
                ServiceManager.Services.AddService(new ProjectService());
                //ServiceManager.Services.AddService( new TestLoader() );
                ServiceManager.Services.AddService(new AddinRegistry());
                ServiceManager.Services.AddService(new AddinManager());
                ServiceManager.Services.AddService(new TestAgency());

                // Initialize Services
                ServiceManager.Services.InitializeServices();

                CoreExtensions.Host.InitializeService();
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