using System;

namespace concord.Builders.TestRunBuilders
{
    public class TestRunAction
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public Func<int> RunTests { get; set; }
    }
}