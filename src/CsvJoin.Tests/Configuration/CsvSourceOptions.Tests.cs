using FluentAssertions;

using CsvJoin.Configuration;

namespace CsvJoin.Tests.Configuration;

public class CsvSourceOptionsTests
{
    [Fact(DisplayName = "CsvSourceOptions has expected default values.")]
    [Trait("Category", "Unit")]
    public void CsvSourceOptionsHasExpectedDefaultValues()
    {
        // Arrange
        var sut = new CsvSourceOptions { FilePath = "file.csv" };

        // Act
        var delimiter = sut.Delimiter;

        // Assert
        delimiter.Should().Be(",");
        sut.Encoding.Should().Be("utf-8");
        sut.TrimFields.Should().BeFalse();
        sut.NullValues.Should().BeEmpty();
        sut.Quote.Should().Be("\"");
        sut.IgnoreBlankLines.Should().BeTrue();
    }
}
