using System.Linq;
using concord.Output;
using FluentAssertions;
using NUnit.Framework;
using Rhino.Mocks;

namespace concord.Tests.Parsing
{
    [TestFixture]
    public class ProgressDisplayTests
    {
        private ProgressDisplay _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = MockRepository.GeneratePartialMock<ProgressDisplay>();
        }

        [Test]
        public void Should_work_for_this_data()
        {
            //Arrange
            var data = new[] {2, 2, 1, 3, 2, 3, 1, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3, 2, 2, 2, 2, 2, 3, 3, 1, 3, 3, 3, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3, 2, 2, 2, 1, 2, 3, 3, 2, 1, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 2};
            var stats = BruteLoadProgressStats(data);
            var indicatorPos = 0;

            //Act
            var display = _classUnderTest.BuildProgressDisplay(80, stats, ref indicatorPos, true);

            //Assert
            display.Length.Should().Be(76);
            display.Should().Be("[xxxxxxxxxxxx========================================================******|]");
        }

        [Test]
        public void Should_have_correct_character_counts_for_this_data()
        {
            //Arrange
            var data = new[] { 2, 2, 1, 3, 2, 3, 1, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3, 2, 2, 2, 2, 2, 3, 3, 1, 3, 3, 3, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3, 2, 2, 2, 1, 2, 3, 3, 2, 1, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 2 };
            var stats = BruteLoadProgressStats(data);
            var screenWidth = 80;
            var indicatorPos = 0;
            var displayRatio = (double) (screenWidth - 4) / data.Length;

            //Act
            var display = _classUnderTest.BuildProgressDisplay(screenWidth, stats, ref indicatorPos, true);

            //Assert
            stats.GetProgressCount(ProgressState.Running).Should().Be(8);
            stats.GetProgressCount(ProgressState.Finished).Should().Be(65);
            stats.GetProgressCount(ProgressState.TestFailure).Should().Be(14);
            //(8 * displayRatio).Dump();
            //(65 * displayRatio).Dump();
            //(14 * displayRatio).Dump();

            display.Count(x => x == 'x' || x == '=').Should().Be(stats.GetCompletedCount(displayRatio));
            display.Count(x => x == '*' || x == '|').Should().Be(7);
            display.Count(x => x == 'x').Should().Be(12);
            display.Count(x => x == '=').Should().Be(57);
            ("[]".Length + 7 + 12 + 56).Should().Be(76);
            //display.Count(x => x == 'x').Should().Be();
            display.Should().Be("[xxxxxxxxxxxx========================================================******|]");
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