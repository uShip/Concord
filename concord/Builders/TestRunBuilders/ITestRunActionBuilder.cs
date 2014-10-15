using concord.Configuration;
using NUnit.Core;

namespace concord.Builders.TestRunBuilders
{
    public interface ITestRunActionBuilder
    {
        void Configure(string assemblyLocation, IRunnerSettings runnerSettings);
        TestRunAction BuildTestRunAction(string name, int index, ITestFilter filter);
        TestRunAction BuildTestRunAction(string name, int index, string testFixture);
    }
}