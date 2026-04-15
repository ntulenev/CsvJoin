using FluentAssertions;

using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class SelectColumnTests
{
    [Fact(DisplayName = "SelectColumn stores constructor values.")]
    [Trait("Category", "Unit")]
    public void SelectColumnStoresConstructorValues()
    {
        // Arrange
        // Act
        var sut = new SelectColumn("right", "Status", "TargetStatus", true, "Unknown");

        // Assert
        sut.SourceAlias.Should().Be("right");
        sut.SourceField.Should().Be("Status");
        sut.OutputField.Should().Be("TargetStatus");
        sut.IsWildcard.Should().BeTrue();
        sut.DefaultValue.Should().Be("Unknown");
    }

    [Fact(DisplayName = "SelectColumn wildcard defaults to false.")]
    [Trait("Category", "Unit")]
    public void SelectColumnWildcardDefaultsToFalse()
    {
        // Arrange
        // Act
        var sut = new SelectColumn("left", "Id", "Id");

        // Assert
        sut.IsWildcard.Should().BeFalse();
        sut.DefaultValue.Should().BeNull();
    }
}
