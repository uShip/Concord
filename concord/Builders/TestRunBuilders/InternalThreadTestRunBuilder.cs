using System;
using concord.Configuration;
using NUnit.Core;

namespace concord.Builders.TestRunBuilders
{
    [Obsolete("DECIDE BETWEEN THIS AND CategoryRunner", true)]
    public class InternalThreadTestRunBuilder : ITestRunBuilder
    {
        public void Configure(string assemblyLocation, IRunnerSettings runnerSettings)
        {
            throw new System.NotImplementedException();
        }

        public int BuildFilteredBlockingProcess(string category, ITestFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public int BuildFilteredBlockingProcess(string testFixture)
        {
            throw new System.NotImplementedException();
        }
    }
}