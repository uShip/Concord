using concord.Output;
using FluentAssertions;
using NUnit.Framework;

namespace concord.Tests.Parsing
{
    [TestFixture]
    public class ProgressStatsTests
    {
        [Test]
        public void Should_count_stats_correctly()
        {
            //Arrange
            var data = new[] { 2, 2, 1, 3, 2, 3, 1, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3, 2, 2, 2, 2, 2, 3, 3, 1, 3, 3, 3, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3, 2, 2, 2, 1, 2, 3, 3, 2, 1, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 2 };

            //Act
            var stats = BruteLoadProgressStats(data);

            //Assert
            stats.GetProgressCount(ProgressState.NotStarted).Should().Be(0);
            stats.GetProgressCount(ProgressState.Running).Should().Be(8);
            stats.GetProgressCount(ProgressState.Finished).Should().Be(65);
            stats.GetProgressCount(ProgressState.TestFailure).Should().Be(14);
            stats.GetProgressCount(ProgressState.RunFailure).Should().Be(0);
        }

        [Test]
        public void Should_count_completed_correctly()
        {
            //Arrange
            var data = new[] { 2, 2, 1, 3, 2, 3, 1, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3, 2, 2, 2, 2, 2, 3, 3, 1, 3, 3, 3, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3, 2, 2, 2, 1, 2, 3, 3, 2, 1, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 2 };
            var stats = BruteLoadProgressStats(data);

            //Act
            var completedCount = stats.GetCompletedCount();

            //Assert
            completedCount.Should().Be(65 + 14);
        }

        [TestCase(0.1646), Ignore("For now idk")]
        [TestCase(0.3)]
        public void Should_ratio_completed_correctly(double displayRatio)
        {
            //Arrange
            var data = new[] { 2, 2, 1, 3, 2, 3, 1, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3, 2, 2, 2, 2, 2, 3, 3, 1, 3, 3, 3, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3, 2, 2, 2, 1, 2, 3, 3, 2, 1, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 2 };
            var stats = BruteLoadProgressStats(data);

            //Act
            var completedCount = stats.GetCompletedCount(displayRatio);

            //Assert
            completedCount.Should().Be((int)((65 + 14) * displayRatio));
        }

        ProgressStats BruteLoadProgressStats(int[] data)
        {
            var stats = new ProgressStats(data.Length);

            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < data[i]; j++)
                {
                    stats.IncrementIndex(i);
                }
            }

            return stats;
        }
    }
}