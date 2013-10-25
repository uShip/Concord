namespace concord.Configuration
{
    internal interface ISettings
    {
        string NunitPath { get; }
        string NunitReportGeneratorPath { get; }
    }
}