using System;
using concord.Builders.TestRunBuilders;
using concord.Builders.ThreadingManagers;

namespace concord.Configuration
{
    public interface ISettingsBasedServiceFactory
    {
        IParallelManager GetParallelManager();
        ITestRunActionBuilder GetTestRunActionBuilder(string assemblyLocation);
    }

    class SettingsBasedServiceFactory : ISettingsBasedServiceFactory
    {
        private readonly IRunnerSettings _runnerSettings;

        public SettingsBasedServiceFactory(IRunnerSettings runnerSettings)
        {
            _runnerSettings = runnerSettings;
        }

        public IParallelManager GetParallelManager()
        {
            switch (_runnerSettings.ThreadingType)
            {
                case ParallelizationMethod.UseTaskParallel:
                    return new RunOnTaskParallel();
                case ParallelizationMethod.UseDotNetThreadPool:
                    return new RunOnThreadPool();
                default:
                    throw new Exception("ParallelManager to use is not configured");
            }
        }

        public ITestRunActionBuilder GetTestRunActionBuilder(string assemblyLocation)
        {
            ITestRunActionBuilder testRunActionBuilder;
            switch (_runnerSettings.TestActionType)
            {
                case TestActionType.ExternalProcesses:
                    testRunActionBuilder = new BlockingProcessTestRunActionBuilder();
                    break;
                case TestActionType.InternalThreads:
                    testRunActionBuilder = new InternalTestRunActionBuilder();
                    break;
                default:
                    throw new Exception("TestActionType to use is not configured");
            }
            testRunActionBuilder.Configure(assemblyLocation, _runnerSettings);
            return testRunActionBuilder;
        }
    }
}