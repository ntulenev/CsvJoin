using FluentAssertions;

using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class CsvDataRowTests
{
    [Fact(DisplayName = "CsvDataRow stores constructor values.")]
    [Trait("Category", "Unit")]
    public void CsvDataRowStoresConstructorValues()
    {
        // Arrange
        var values = new Dictionary<string, string?> { ["Id"] = "1" };

        // Act
        var sut = new CsvDataRow(5, values);

        // Assert
        sut.Index.Should().Be(5);
        sut.Values.Should().BeSameAs(values);
    }

    [Fact(DisplayName = "CsvDataRow GetJoinKey returns empty string for null value.")]
    [Trait("Category", "Unit")]
    public void GetJoinKeyReturnsEmptyStringForNullValue()
    {
        // Arrange
        var sut = new CsvDataRow(0, new Dictionary<string, string?> { ["Id"] = null });

        // Act
        var result = sut.GetJoinKey("Id");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact(DisplayName = "CsvDataRow GetValueOrDefault returns fallback for missing value.")]
    [Trait("Category", "Unit")]
    public void GetValueOrDefaultReturnsFallbackForMissingValue()
    {
        // Arrange
        var sut = new CsvDataRow(0, new Dictionary<string, string?>());

        // Act
        var result = sut.GetValueOrDefault("Status", "Unknown");

        // Assert
        result.Should().Be("Unknown");
    }
}
