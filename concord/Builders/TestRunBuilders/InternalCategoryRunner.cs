using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Core;
using NUnit.Util;

namespace concord.Builders.TestRunBuilders
{
    [Obsolete("DECIDE BETWEEN THIS AND ITestRunBuilder", true)]
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