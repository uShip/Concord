namespace concord.Configuration
{
    public interface IRunnerSettingsBuilder
    {
        IRunnerSettingsBuilder SetOutputFolder(string outputFolder);
        IRunnerSettingsBuilder PrependFilenames(string prependWith);

        IRunnerSettingsBuilder SetResultsXmlFilename(string filename);
        IRunnerSettingsBuilder SetResultsHtmlReportFilename(string filename);
        IRunnerSettingsBuilder SetResultsStatsFilename(string filename);

        IRunnerSettingsBuilder SetNamespace(string ns);
        IRunnerSettingsBuilder RerunFailedCategories(bool rerun);

        IRunnerSettingsBuilder RunUncategorizedTestFixturesParallel(bool runParallel = true);
        IRunnerSettingsBuilder IncludeIgnoredFeatures(bool includeIgnored);

        IParallelizationSettingsBuilder ForParallelization();
        ITestRunActionSettingsBuilder ForTestRunAction();

        RunnerSettings Build();
    }

    public interface IParallelizationSettingsBuilder
    {
        IRunnerSettingsBuilder UseTaskParallel();
        IRunnerSettingsBuilder UseDotNetThreadPool();
    }

    public interface ITestRunActionSettingsBuilder
    {
        IRunnerSettingsBuilder UseExternalProcesses();
        IRunnerSettingsBuilder UseInternalThreads();
    }
}