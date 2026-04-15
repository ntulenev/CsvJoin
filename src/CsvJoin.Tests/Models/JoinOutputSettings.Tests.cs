using FluentAssertions;

using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class JoinOutputSettingsTests
{
    [Fact(DisplayName = "JoinOutputSettings stores constructor values.")]
    [Trait("Category", "Unit")]
    public void JoinOutputSettingsStoresConstructorValues()
    {
        // Arrange
        // Act
        var sut = new JoinOutputSettings("results", ";", 25, true);

        // Assert
        sut.ResultsDirectory.Should().Be("results");
        sut.Delimiter.Should().Be(";");
        sut.ConsoleMaxRows.Should().Be(25);
        sut.OpenResultAfterBuild.Should().BeTrue();
    }
}
