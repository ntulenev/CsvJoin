using FluentAssertions;

using CsvJoin.Configuration;

namespace CsvJoin.Tests.Configuration;

public class AppSettingsTests
{
    [Fact(DisplayName = "AppSettings has expected default values.")]
    [Trait("Category", "Unit")]
    public void AppSettingsHasExpectedDefaultValues()
    {
        // Arrange
        var sut = new AppSettings { Query = "SELECT left.Id FROM left INNER JOIN right ON left.Id = right.Id" };

        // Act
        var sources = sut.Sources;
        var output = sut.Output;

        // Assert
        sources.Should().NotBeNull().And.BeEmpty();
        output.Should().NotBeNull();
    }
}
