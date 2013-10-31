namespace concord.Output
{
    public interface IProgressDisplay
    {
        string BuildProgressDisplay(int width, int[] runningTests, ref int indicatorPos);
        char ArrayValueToRunningStatusChar(int value);
    }
}