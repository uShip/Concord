namespace concord.Configuration
{
    public interface IRunnerSettings
    {
        string OutputBasePath { get; set; }

        string ResultsXmlFilename { set; }
        string ResultsHtmlReportFilename { set; }
        string ResultsStatsFilename { set; }
        string Namespace { get; set; }

        string ResultsXmlFilepath { get; }
        string ResultsHtmlReportFilepath { get; }
        string ResultsStatsFilepath { get; }
        string ResultsOrderDataFilepath { get; }

        bool RunUncategorizedTestFixturesParallel { get; set; }
        bool IncludeIgnoredFeatures { get; set; }
        bool RerunFailedCategories { get; set; }
        bool DisplayFailureSymbolsInProgressDisplay { get; set; }

        ParallelizationMethod ThreadingType { get; set; }
        TestActionType TestActionType { get; set; }

        ISettingsBasedServiceFactory GetServiceFactory();
    }
}