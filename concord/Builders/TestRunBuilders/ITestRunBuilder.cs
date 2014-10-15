using concord.Configuration;
using NUnit.Core;

namespace concord.Builders.TestRunBuilders
{
    public interface ITestRunBuilder
    {
        void Configure(string assemblyLocation, IRunnerSettings runnerSettings);
        int BuildFilteredBlockingProcess(string category, ITestFilter filter);
        int BuildFilteredBlockingProcess(string testFixture);
    }
}