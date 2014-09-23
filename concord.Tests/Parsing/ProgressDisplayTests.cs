using System.Collections.Generic;
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
            display.Should().Be("[xxxxxxxxxxxx=========================================================******|]");
        }

        [TestCase(23, 53, 63, 13, 34, "[?????????????xxxxx==========================*********************|··········]", null, TestName = "Random", Description = "Random numbers, assuming correct output")]
        [TestCase(1, 53, 63, 13, 34, "[???????????????xxxxxx=============================************************|·]", null, TestName = "One not started")]
        [TestCase(23, 1, 63, 13, 34, "[???????????????????xxxxxxx====================================|·············]", null, TestName = "One running")]
        [TestCase(23, 53, 1, 13, 34, "[????????????????????xxxxxxx=********************************|···············]", null, TestName = "One success")]
        [TestCase(23, 53, 63, 1, 34, "[??????????????x============================***********************|·········]", null, TestName = "One failure")]
        [TestCase(23, 53, 63, 13, 1, "[?xxxxxx===============================**************************|···········]", null, TestName = "One unknown")]
        [TestCase(144, 0, 0, 0, 0,  "[············································································]", 144, TestName = "Scenario 1.01")]
        [TestCase(133, 11, 0, 0, 0, "[*****|······································································]", 144, TestName = "Scenario 1.02")]
        [TestCase(133, 10, 1, 0, 0, "[=*****|·····································································]", 144, TestName = "Scenario 1.03")]
        [TestCase(132, 11, 1, 0, 0, "[=*****|·····································································]", 144, TestName = "Scenario 1.04")]
        [TestCase(130, 11, 2, 1, 0, "[x=*****|····································································]", 144, TestName = "Scenario 1.05")]
        [TestCase(130, 9, 3, 2, 0,  "[x==****|····································································]", 144, TestName = "Scenario 1.06")]
        [TestCase(128, 11, 3, 2, 0,  "[x==*****|···································································]", 144, TestName = "Scenario 1.07")]
        [TestCase(126, 11, 5, 2, 0,  "[x===*****|··································································]", 144, TestName = "Scenario 1.08")]
        [TestCase(109, 0, 0, 0, 0, "[············································································]", 109, TestName = "Scenario 2.01", Description = "Simulating the starting of an odd ratio one (76/109)")]
        [TestCase(98, 11, 0, 0, 0, "[*******|····································································]", 109, TestName = "Scenario 2.02")]
        [TestCase(98, 10, 1, 0, 0, "[=******|····································································]", 109, TestName = "Scenario 2.03")]
        [TestCase(97, 11, 1, 0, 0, "[=*******|···································································]", 109, TestName = "Scenario 2.04")]
        [TestCase(95, 11, 2, 1, 0, "[x=*******|··································································]", 109, TestName = "Scenario 2.05")]
        [TestCase(95, 9, 3, 2, 0, "[x==******|··································································]", 109, TestName = "Scenario 2.06")]
        [TestCase(93, 11, 3, 2, 0, "[x==*******|·································································]", 109, TestName = "Scenario 2.07")]
        [TestCase(91, 11, 5, 2, 0, "[x===*******|································································]", 109, TestName = "Scenario 2.08")]

        [TestCase(9, 11, 85, 4, 0, "[xx===========================================================*******|·······]", 109, TestName = "Scenario 2.34", Description = "Simulating the finishing of an odd ratio one (76/109)")]
        [TestCase(8, 11, 86, 4, 0, "[xx============================================================*******|······]", 109, TestName = "Scenario 2.35")]
        [TestCase(7, 11, 87, 4, 0, "[xx=============================================================*******|·····]", 109, TestName = "Scenario 2.36")]
        [TestCase(6, 11, 88, 4, 0, "[xx=============================================================*******|·····]", 109, TestName = "Scenario 2.37")]
        [TestCase(5, 11, 89, 4, 0, "[xx==============================================================*******|····]", 109, TestName = "Scenario 2.38")]
        [TestCase(3, 12, 90, 4, 0, "[xx===============================================================********|··]", 109, TestName = "Scenario 2.39")]
        [TestCase(2, 12, 91, 4, 0, "[xx===============================================================********|··]", 109, TestName = "Scenario 2.40")]
        [TestCase(1, 12, 92, 4, 0, "[xx================================================================********|·]", 109, TestName = "Scenario 2.41")]
        [TestCase(1, 11, 93, 4, 0, "[xx=================================================================*******|·]", 109, TestName = "Scenario 2.42")]
        [TestCase(1, 10, 94, 4, 0, "[xx==================================================================******|·]", 109, TestName = "Scenario 2.43")]
        [TestCase(1, 09, 95, 4, 0, "[xx==================================================================******|·]", 109, TestName = "Scenario 2.44")]
        [TestCase(1, 08, 96, 4, 0, "[xx===================================================================*****|·]", 109, TestName = "Scenario 2.45")]
        [TestCase(1, 07, 97, 4, 0, "[xx====================================================================****|·]", 109, TestName = "Scenario 2.46")]
        [TestCase(1, 06, 98, 4, 0, "[xx====================================================================****|·]", 109, TestName = "Scenario 2.47")]
        [TestCase(1, 05, 99, 4, 0, "[xx=====================================================================***|·]", 109, TestName = "Scenario 2.48")]
        [TestCase(1, 4, 100, 4, 0, "[xx======================================================================**|·]", 109, TestName = "Scenario 2.49")]
        
        [TestCase(1, 4, 100, 4, 0, "[xx======================================================================**|·]", 109, TestName = "Scenario 2.50")]
        [TestCase(0, 5, 100, 4, 0, "[xx======================================================================***|]", 109, TestName = "Scenario 2.51")]
        [TestCase(0, 4, 101, 4, 0, "[xx=======================================================================**|]", 109, TestName = "Scenario 2.52")]
        [TestCase(0, 3, 102, 4, 0, "[xx=======================================================================**|]", 109, TestName = "Scenario 2.53")]
        [TestCase(0, 2, 103, 4, 0, "[xx========================================================================*|]", 109, TestName = "Scenario 2.54")]
        [TestCase(0, 1, 104, 4, 0, "[xx=========================================================================|]", 109, TestName = "Scenario 2.55")]
        [TestCase(0, 0, 105, 4, 0, "[xx==========================================================================]", 109, TestName = "Scenario 2.56")]
        public void Should_work_for_this_data(int notStarted, int running, int success, int testFailure, int runFailure, string expected, int? expectedCount)
        {
            //Arrange
            var sortedData = BuildProgressStats(notStarted, running, success, testFailure, runFailure);
            var stats = BruteLoadProgressStats(sortedData.Cast<int>().ToArray());
            var indicatorPos = 0;

            var screenWidth = 80;
            var width = screenWidth - 2;

            //Act
            var display = _classUnderTest.BuildProgressDisplay(screenWidth, stats, ref indicatorPos, true);

            //Assert
            if (expectedCount.HasValue)
            {
                expectedCount.Should().Be(notStarted + running + success + testFailure + runFailure, "Total sum incorrect");
            }
            display.Should().Be(expected);
            display.Length.Should().Be(width);
        }

        [Test]
        public void Should_work_for_this_data_with_one_not_started()
        {
            //Arrange
            var data = new[] { 0, 2, 1, 3, 2, 3, 1, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3, 2, 2, 2, 2, 2, 3, 3, 1, 3, 3, 3, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3, 2, 2, 2, 1, 2, 3, 3, 2, 1, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 2 };
            var stats = BruteLoadProgressStats(data);
            var indicatorPos = 0;

            //Act
            var display = _classUnderTest.BuildProgressDisplay(80, stats, ref indicatorPos, true);

            //Assert
            display.Should().Be("[xxxxxxxxxxxx========================================================******|·]");
        }

        [Test]
        public void Should_work_for_this_data_with_one_not_started_fewer_failures()
        {
            //Arrange
            var data = new[] { 0, 2, 1, 2, 2, 2, 1, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 3, 3, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 3, 2, 2, 2, 1, 2, 3, 3, 2, 1, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 2, 2, 2 };
            var stats = BruteLoadProgressStats(data);
            var indicatorPos = 0;

            //Act
            var display = _classUnderTest.BuildProgressDisplay(80, stats, ref indicatorPos, true);

            //Assert
            display.Should().Be("[xxxxxx==============================================================******|·]");
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

            display.Length.Should().Be(78, "leave a couple characters off the max width");
            //Meh ignore this: display.Count(x => x == 'x' || x == '=').Should().Be(stats.GetCompletedCount(displayRatio));
            display.Count(x => x == '*' || x == '|').Should().Be(7);
            display.Count(x => x == 'x').Should().Be(12);
            display.Count(x => x == '=').Should().Be(57);
            ("[]".Length + 7 + 12 + 56).Should().Be(77);
            //display.Count(x => x == 'x').Should().Be();
            //                   [xxxxxxxxxxxx========================================================******|·]
            display.Should().Be("[xxxxxxxxxxxx=========================================================******|]");
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

        IEnumerable<ProgressState> BuildProgressStats(int notStarted, int running, int success, int testFailure, int runFailure)
        {
            for (int i = 0; i < notStarted; i++)
                yield return ProgressState.NotStarted;
            for (int i = 0; i < running; i++)
                yield return ProgressState.Running;
            for (int i = 0; i < success; i++)
                yield return ProgressState.Finished;
            for (int i = 0; i < testFailure; i++)
                yield return ProgressState.TestFailure;
            for (int i = 0; i < runFailure; i++)
                yield return ProgressState.RunFailure;
        }
    }
}