namespace concord.Nunit
{
    public interface IResultMerger
    {
        MergedResults MergeResults(string path);
    }
}