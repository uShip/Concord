namespace concord.Output
{
    public interface IProgressDisplay
    {
        string BuildProgressDisplay(int width, ProgressStats runningTests, ref int indicatorPos, bool displayFailureSymbols);
        char ArrayValueToRunningStatusChar(ProgressState value);
    }
}