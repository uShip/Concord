namespace concord.Output
{
    public enum ProgressState : int
    {
        /// <summary>
        /// The test has not started
        /// </summary>
        NotStarted = 0,

        /// <summary>
        /// The test is running
        /// </summary>
        Running = 1,

        /// <summary>
        /// The test finished (successfully)
        /// </summary>
        Finished = 2,

        /// <summary>
        /// This means the test threw an exception, or otherwise asserted failure
        /// </summary>
        TestFailure = 3,

        /// <summary>
        /// This means something with the test runner failed
        /// </summary>
        RunFailure = 4
    }
}