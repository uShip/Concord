namespace concord.Configuration
{
    public interface IRunnerSettingsBuilder
    {
        RunnerSettingsBuilder SetOutputFolder(string outputFolder);
        RunnerSettingsBuilder PrependFilenames(string prependWith);

        RunnerSettingsBuilder SetResultsXmlFilename(string filename);
        RunnerSettingsBuilder SetResultsHtmlReportFilename(string filename);
        RunnerSettingsBuilder SetResultsStatsFilename(string filename);

        RunnerSettings Build();
    }
}