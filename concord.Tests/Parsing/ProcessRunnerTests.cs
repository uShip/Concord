using System;
using System.Collections.Generic;
using System.Linq;
using concord.Builders;
using concord.Output;
using FluentAssertions;
using NUnit.Framework;
using Rhino.Mocks;

namespace concord.Tests.Parsing
{
    public class ProcessRunnerTests
    {
        //private RhinoAutoMocker<ProcessRunner> _services;

        //private ProcessRunner ClassUnderTest
        //{
        //    get { return _services.ClassUnderTest; }
        //}
        private ProcessRunner ClassUnderTest;

        private IResultsOrderService _resultsOrderService = MockRepository.GenerateMock<IResultsOrderService>();

        [SetUp]
        public void SetUp()
        {
            ////_services = new RhinoAutoMocker<ProcessRunner>();
            //ObjectFactory.Configure(x => x.AddRegistry<RunnerRegistry>());
            //_services = new RhinoAutoMocker<ProcessRunner>();

            ClassUnderTest = MockRepository.GeneratePartialMock<ProcessRunner>(
                MockRepository.GenerateMock<IResultsWriter>(),
                MockRepository.GenerateMock<IProgressDisplay>(),
                MockRepository.GenerateMock<IResultsStatsWriter>(),
                _resultsOrderService);
        }

        [Test]
        public void Should_get_tests_in_desired_order()
        {
            //Arrange
            MockBuildAllActions();

            var testFixtures = GetRandomStringList(10, 6, 12).ToList();
            var runnableCategories = GetRandomStringList(25, 6, 12).ToList();

            var desiredOrder = runnableCategories.OrderBy(x => Guid.NewGuid()).Take(20).ToList();

            _resultsOrderService.Stub(x => x.GetCategoriesInDesiredOrder())
                                .Return(desiredOrder);

            //Act
            var outputItems = ClassUnderTest.BuildSortedAllActions(testFixtures, runnableCategories);
            var outputNames = outputItems.Select(x => x.Name);

            //Assert

            //Current Rules:
            //** "all" option in this test case is not in the desiredOrder (since random names) so it will be first
            outputNames.First().Should().Be("all");
            //  any un-ordered categories go at the start, in their original order...
            outputNames.Skip(1).Should().ContainInOrder(runnableCategories.Except(desiredOrder));
            var rest = runnableCategories.Count + 1 - 20;
            //  any in desiredOrder should come next
            outputNames.Skip(rest).Count().Should().Be(20);
            outputNames.Skip(rest).Take(20).Should().ContainInOrder(desiredOrder, "test run desired order");

            ////Previous Rules:
            ////  any in desiredOrder should come first
            //outputNames.Take(20).Should().ContainInOrder(desiredOrder, "test run desired order");
            ////** "all" option in this test case is not in the desiredOrder (since random names) so it will be next
            //outputNames.Skip(20).First().Should().Be("all");
            ////  any un-ordered categories go at the end, in their original order...
            //outputNames.Skip(21).Should().ContainInOrder(runnableCategories.Except(desiredOrder));
        }

        private void MockBuildAllActions()
        {
            ClassUnderTest.Stub(x => x.BuildAllActions(null, null))
                           .IgnoreArguments()
                           .WhenCalled(invocation =>
                           {
                               var testFixtures = invocation.Arguments[0] as IEnumerable<string>;
                               var runnableCategories = invocation.Arguments[1] as IEnumerable<string>;

                               var returnList = new List<ProcessRunner.TestRunAction>();

                               var indexOffset = 0;
                               if (testFixtures.Any())
                               {
                                   returnList.Add(
                                       new ProcessRunner.TestRunAction
                                       {
                                           Name = "all",
                                           Index = 0,
                                           RunTests = () => { throw new Exception("This is a test func"); }
                                       });
                                   indexOffset = 1;
                               }

                               //Categories:
                               returnList.AddRange(
                                   from cat in runnableCategories.Select((x, i) => new { Name = x, Index = i + indexOffset })
                                   let x = cat.Name
                                   select new ProcessRunner.TestRunAction
                                   {
                                       Name = x,
                                       Index = cat.Index,
                                       RunTests = () => { throw new Exception("This is a test func"); }
                                   });

                               invocation.ReturnValue = returnList;
                           });
        }

        readonly Random _rand = new Random();

        private string GetRandomString(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(
                Enumerable.Repeat(chars, length)
                          .Select(s => s[_rand.Next(s.Length)])
                          .ToArray());
        }

        private IEnumerable<string> GetRandomStringList(int items, int minLength = 8, int maxLength = 8)
        {
            for (var i = 0; i < items; ++i)
                yield return GetRandomString(
                    _rand.Next(minLength, maxLength + 1));
        }
    }
}