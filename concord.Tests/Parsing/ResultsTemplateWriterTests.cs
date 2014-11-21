using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using concord.Output;
using concord.Output.Dto;
using concord.RazorTemplates.Models;
using concord.Tests.Framework;
using FluentAssertions;
using NUnit.Framework;
using RazorEngine.Templating;
using Rhino.Mocks;

namespace concord.Tests.Parsing
{
    [TestFixture]
    public class ResultsTemplateWriterTests : InteractionContext<ResultsTemplateWriter>
    {
        //private ResultsTemplateWriter _classUnderTest;
        private TemplateService _templateService;

        [SetUp]
        public void SetUp()
        {
            _templateService = new TemplateService();
            //_classUnderTest = MockRepository.GeneratePartialMock<ResultsTemplateWriter>(_templateService);

            UseInstanceFor<ITemplateService>(_templateService);
            MockFor<IHtmlGanttChart>()
                .Stub(x => x.SeperateIntoLines(null))
                .IgnoreArguments()
                .WhenCalled(x =>
                {
                    var list = (IEnumerable<RunStats>) x.Arguments[0];
                    if (list == null)
                    {
                        x.ReturnValue = new List<List<RunStats>>();
                        return;
                    }
                    x.ReturnValue = list
                        .GroupBy(g => g.GetHashCode() % 4)
                        .Select(g => g.ToList())
                        .ToList();
                })
                .Return(null);
        }

        [Test]
        public void Should_build_template()
        {
            //Arrange
            
            //Act
            var fancyResults = new FancyResults { TotalRuntime = TimeSpan.FromSeconds(78274) };
            var output = ClassUnderTest.OutputResults(fancyResults);

            //Assert
            output.Should().NotBeNullOrWhiteSpace();
            output.Should().Contain(fancyResults.TotalRuntime.ToString());

            Debug.WriteLine(output);
        }
    }
}