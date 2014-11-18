using System;
using concord.Output;
using concord.RazorTemplates.Models;
using FluentAssertions;
using NUnit.Framework;
using RazorEngine.Templating;
using Rhino.Mocks;

namespace concord.Tests.Parsing
{
    [TestFixture]
    public class ResultsTemplateWriterTests
    {
        private ResultsTemplateWriter _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = MockRepository.GeneratePartialMock<ResultsTemplateWriter>(new TemplateService());
        }

        [Test]
        public void Should_build_template()
        {
            //Arrange
            
            //Act
            var fancyResults = new FancyResults { TotalRuntime = TimeSpan.FromSeconds(78274) };
            var output = _classUnderTest.OutputResults(fancyResults);

            //Assert
            output.Should().NotBeNullOrWhiteSpace();
            output.Should().Contain(fancyResults.TotalRuntime.ToString());
        }

    }
}