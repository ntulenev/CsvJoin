using FluentAssertions;

using CsvJoin.Configuration;

namespace CsvJoin.Tests.Configuration;

public class CsvSourceOptionsTests
{
    [Fact(DisplayName = "CsvSourceOptions has expected default delimiter.")]
    [Trait("Category", "Unit")]
    public void CsvSourceOptionsHasExpectedDefaultDelimiter()
    {
        // Arrange
        var sut = new CsvSourceOptions { FilePath = "file.csv" };

        // Act
        var delimiter = sut.Delimiter;

        // Assert
        delimiter.Should().Be(",");
    }
}
