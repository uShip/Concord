using concord.Configuration;
using NUnit.Core;

namespace concord.Builders.TestRunBuilders
{
    class BlockingProcessTestRunActionBuilder : BlockingProcessBuilder, ITestRunActionBuilder
    {
        public TestRunAction BuildTestRunAction(string name, int index, ITestFilter filter)
        {
            return new TestRunAction
            {
                Name = name,
                Index = index,
                RunTests = () => BuildFilteredBlockingProcess(name, filter)
            };
        }

        public TestRunAction BuildTestRunAction(string name, int index, string testFixture)
        {
            return new TestRunAction
            {
                Name = name,
                Index = index,
                RunTests = () => BuildFilteredBlockingProcess(testFixture)
            };
        }
    }
}