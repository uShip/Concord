using System;
using System.Diagnostics;
using System.IO;
using NUnit.ConsoleRunner;
using NUnit.Core;
using NUnit.Util;

namespace concord.Builders.TestRunBuilders
{
    public class CategoryRunner : EventListener, IDisposable
    {
        private readonly ITestFilter Filter;
        private readonly string _outputXmlFile;
        private readonly string _assemblyLocation;
        private readonly Stopwatch _timer = new Stopwatch();

        private TestRunner _testRunner;

        public CategoryRunner(
            string assemblyLocation,
            ITestFilter filter,
            string outputXmlFile)
        {
            _assemblyLocation = assemblyLocation;
            Filter = filter;
            _outputXmlFile = outputXmlFile;

            MarkAsInitalized();
        }

        public bool FinishedRunning { private set; get; }
        public bool IsRunning { private set; get; }

        public TimeSpan RunningTime
        {
            get { return _timer.Elapsed; }
        }


        public int Execute()
        {
            try
            {
                var result = RunTesterBlocking();
                var summary = new ResultSummarizer(result);

                return summary.Errors + summary.Failures + summary.NotRunnable;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return ConsoleUi.FILE_NOT_FOUND;
            }
            catch (Exception ex)
            {
                //TODO need to expand on error cases better... mimic console runner
                Console.WriteLine(ex);
                return ConsoleUi.UNEXPECTED_ERROR;
            }
        }

        public TestResult RunTesterBlocking()
        {
            using (_testRunner = GetTestRunner())
            {
                //MarkAsRunning();
                var results = _testRunner.Run(this, Filter, false, LoggingThreshold.Off);
                //MarkAsFinished();

                //TESTING:
                if (!FinishedRunning) throw new Exception("NEED TO CALL MarkAsFinished!!!");

                return results;
            }
        }

        public void RunTesterAsync()
        {
            _testRunner = GetTestRunner();

            _testRunner.BeginRun(this, Filter, false, LoggingThreshold.Off);
        }

        private void MarkAsInitalized()
        {
            FinishedRunning = false;
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
        }

        public void Dispose()
        {
            if (_testRunner != null)
                _testRunner.Dispose();
        }

        public TestRunner GetTestRunner()
        {
            lock (InitalizeLock)
            {
                if (!CoreExtensions.Host.Initialized)
                {
                    InitalizeNUnits();
                }
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

        static readonly object InitalizeLock = new object();
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
                var writer = new XmlResultWriter(_outputXmlFile);
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