using System;
using concord.Builders.TestRunBuilders;
using concord.Builders.ThreadingManagers;

namespace concord.Configuration
{
    public interface ISettingsBasedServiceFactory
    {
        IParallelManager GetParallelManager();
        ITestRunActionBuilder GetTestRunActionBuilder();
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

        public ITestRunActionBuilder GetTestRunActionBuilder()
        {
            switch (_runnerSettings.TestActionType)
            {
                case TestActionType.ExternalProcesses:
                    return new BlockingProcessTestRunActionBuilder();
                case TestActionType.InternalThreads:
                    return new InternalTestRunActionBuilder();
                default:
                    throw new Exception("TestActionType to use is not configured");
            }
        }
    }
}