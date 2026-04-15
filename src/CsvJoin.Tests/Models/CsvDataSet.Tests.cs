using FluentAssertions;

using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class CsvDataSetTests
{
    [Fact(DisplayName = "CsvDataSet can be created and resolve headers case-insensitively.")]
    [Trait("Category", "Unit")]
    public void CsvDataSetCanBeCreatedAndResolveHeadersCaseInsensitively()
    {
        // Arrange
        var headers = new[] { "Id", "Full Name" };
        var rows = new[] { new CsvDataRow(0, new Dictionary<string, string?>()) };

        // Act
        var sut = new CsvDataSet("left", "left.csv", headers, rows);
        var resolvedHeader = sut.ResolveHeader("full name");

        // Assert
        resolvedHeader.Should().Be("Full Name");
        sut.Alias.Should().Be("left");
        sut.FilePath.Should().Be("left.csv");
    }

    [Fact(DisplayName = "CsvDataSet constructor throws when alias is empty.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenAliasIsEmpty()
    {
        // Arrange
        var headers = new[] { "Id" };
        var rows = new[] { new CsvDataRow(0, new Dictionary<string, string?>()) };

        // Act
        Action action = () => _ = new CsvDataSet(" ", "left.csv", headers, rows);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "CsvDataSet constructor throws when file path is empty.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenFilePathIsEmpty()
    {
        // Arrange
        var headers = new[] { "Id" };
        var rows = new[] { new CsvDataRow(0, new Dictionary<string, string?>()) };

        // Act
        Action action = () => _ = new CsvDataSet("left", " ", headers, rows);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "CsvDataSet constructor throws when headers are null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenHeadersAreNull()
    {
        // Arrange
        var rows = new[] { new CsvDataRow(0, new Dictionary<string, string?>()) };

        // Act
        Action action = () => _ = new CsvDataSet("left", "left.csv", null!, rows);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "CsvDataSet constructor throws when rows are null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenRowsAreNull()
    {
        // Arrange
        var headers = new[] { "Id" };

        // Act
        Action action = () => _ = new CsvDataSet("left", "left.csv", headers, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "CsvDataSet ResolveHeader throws for missing header.")]
    [Trait("Category", "Unit")]
    public void ResolveHeaderThrowsForMissingHeader()
    {
        // Arrange
        var sut = new CsvDataSet("left", "left.csv", ["Id"], []);

        // Act
        Action action = () => _ = sut.ResolveHeader("Missing");

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Missing*");
    }

    [Fact(DisplayName = "CsvDataSet BuildLookup groups rows by join key.")]
    [Trait("Category", "Unit")]
    public void BuildLookupGroupsRowsByJoinKey()
    {
        // Arrange
        var rows = new[]
        {
            new CsvDataRow(0, new Dictionary<string, string?> { ["Id"] = "1" }),
            new CsvDataRow(1, new Dictionary<string, string?> { ["Id"] = "1" }),
        };
        var sut = new CsvDataSet("left", "left.csv", ["Id"], rows);

        // Act
        var result = sut.BuildLookup("Id");

        // Assert
        result.Should().ContainKey("1");
        result["1"].Should().HaveCount(2);
    }

    [Fact(DisplayName = "CsvDataSet Bind expands wildcard columns.")]
    [Trait("Category", "Unit")]
    public void BindExpandsWildcardColumns()
    {
        // Arrange
        var sut = new CsvDataSet("left", "left.csv", ["Id", "Name"], []);
        var selectColumn = new SelectColumn("left", "*", "*", true);

        // Act
        var result = sut.Bind(selectColumn, JoinSourceSide.Left);

        // Assert
        result.Should().HaveCount(2);
        result[0].SourceField.Should().Be("Id");
        result[1].SourceField.Should().Be("Name");
    }
}
