using FluentAssertions;

using CsvJoin.Configuration;

namespace CsvJoin.Tests.Configuration;

public class OutputOptionsTests
{
    [Fact(DisplayName = "OutputOptions has expected default values.")]
    [Trait("Category", "Unit")]
    public void OutputOptionsHasExpectedDefaultValues()
    {
        // Arrange
        var sut = new OutputOptions();

        // Act
        var resultsDirectory = sut.ResultsDirectory;
        var delimiter = sut.Delimiter;
        var consoleMaxRows = sut.ConsoleMaxRows;
        var openResultAfterBuild = sut.OpenResultAfterBuild;

        // Assert
        resultsDirectory.Should().Be("results");
        delimiter.Should().Be(",");
        consoleMaxRows.Should().Be(50);
        openResultAfterBuild.Should().BeTrue();
    }
}
