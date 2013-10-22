namespace concord.Builders
{
    public interface IRunner
    {
        string GetRunResultsAsXml();
        string GetRunResultsAsXml(int concurrentRunners);
    }
}