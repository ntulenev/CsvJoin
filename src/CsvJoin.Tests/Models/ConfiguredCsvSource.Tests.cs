using FluentAssertions;

using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class ConfiguredCsvSourceTests
{
    [Fact(DisplayName = "ConfiguredCsvSource exposes option values.")]
    [Trait("Category", "Unit")]
    public void ConfiguredCsvSourceExposesOptionValues()
    {
        // Arrange
        var options = new CsvSourceOptions { FilePath = "left.csv", Delimiter = ";" };

        // Act
        var sut = new ConfiguredCsvSource("left", options);

        // Assert
        sut.Alias.Should().Be("left");
        sut.Options.Should().BeSameAs(options);
        sut.FilePath.Should().Be("left.csv");
        sut.Delimiter.Should().Be(";");
    }
}
