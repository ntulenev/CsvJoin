using FluentAssertions;

using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class BoundSelectColumnTests
{
    [Fact(DisplayName = "BoundSelectColumn stores constructor values.")]
    [Trait("Category", "Unit")]
    public void BoundSelectColumnStoresConstructorValues()
    {
        // Arrange
        // Act
        var sut = new BoundSelectColumn(JoinSourceSide.Right, "Status", "OutputStatus", "Unknown");

        // Assert
        sut.SourceSide.Should().Be(JoinSourceSide.Right);
        sut.SourceField.Should().Be("Status");
        sut.OutputField.Should().Be("OutputStatus");
        sut.DefaultValue.Should().Be("Unknown");
    }

    [Fact(DisplayName = "BoundSelectColumn Project returns fallback when row is missing.")]
    [Trait("Category", "Unit")]
    public void ProjectReturnsFallbackWhenRowIsMissing()
    {
        // Arrange
        var sut = new BoundSelectColumn(JoinSourceSide.Right, "Status", "Status", "Unknown");

        // Act
        var result = sut.Project(new CsvDataRow(0, new Dictionary<string, string?>()), null);

        // Assert
        result.Should().Be("Unknown");
    }
}
