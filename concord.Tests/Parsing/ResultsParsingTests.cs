using FluentAssertions;
using NUnit.Framework;
using Rhino.Mocks;
using concord.Parsers;
using concord.Tests.Properties;
using System.Linq;

namespace concord.Tests.Parsing
{
    [TestFixture]
    public class ResultsParsingTests
    {
        private ResultsParser _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = MockRepository.GeneratePartialMock<ResultsParser>();
        }

        [Test]
        public void Should_be_able_to_parse_output_file_for_errors()
        {
            // Arrange
            var results = Resources.ExampleResults.Split('\n');
            
            // Act
            var categories = _classUnderTest.GetErrorsCategories(results);

            // Assert
            categories.Count().Should().Be(1);
            categories.First().Should().Be("SearchFeature");
        }

        [Test]
        public void Should_be_able_to_get_features_in_correct_order()
        {
            // Arrange
            var results = Resources.ExampleResults.Split('\n');

            // Act
            var categories = _classUnderTest.GetCategoriesInOrder(results);

            // Assert
            categories.Count().Should().Be(70);
            categories.First().Should().Be("ListingsInternational");
        }
    }
}