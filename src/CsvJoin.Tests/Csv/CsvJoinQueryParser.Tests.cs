using FluentAssertions;

using CsvJoin.Csv;
using CsvJoin.Models;

namespace CsvJoin.Tests.Csv;

public class CsvJoinQueryParserTests
{
    [Fact(DisplayName = "CsvJoinQueryParser Parse returns parsed query for valid input.")]
    [Trait("Category", "Unit")]
    public void ParseReturnsParsedQueryForValidInput()
    {
        // Arrange
        var sut = new CsvJoinQueryParser();

        // Act
        var result = sut.Parse("SELECT left.Id, right.[Full Name] AS Name FROM left FULL JOIN right ON left.Id = right.ExternalId");

        // Assert
        result.LeftAlias.Should().Be("left");
        result.LeftJoinField.Should().Be("Id");
        result.RightAlias.Should().Be("right");
        result.RightJoinField.Should().Be("ExternalId");
        result.JoinType.Should().Be(JoinType.Full);
        result.SelectColumns.Should().HaveCount(2);
        result.SelectColumns[1].SourceField.Should().Be("Full Name");
        result.SelectColumns[1].OutputField.Should().Be("Name");
    }

    [Fact(DisplayName = "CsvJoinQueryParser Parse supports wildcard selection.")]
    [Trait("Category", "Unit")]
    public void ParseSupportsWildcardSelection()
    {
        // Arrange
        var sut = new CsvJoinQueryParser();

        // Act
        var result = sut.Parse("SELECT left.*, right.Status FROM left LEFT JOIN right ON left.Id = right.Id");

        // Assert
        result.SelectColumns[0].IsWildcard.Should().BeTrue();
        result.SelectColumns[0].SourceField.Should().Be("*");
    }

    [Fact(DisplayName = "CsvJoinQueryParser Parse supports coalesce default values.")]
    [Trait("Category", "Unit")]
    public void ParseSupportsCoalesceDefaultValues()
    {
        // Arrange
        var sut = new CsvJoinQueryParser();

        // Act
        var result = sut.Parse("SELECT left.Id, COALESCE(right.Status, 'Unknown') AS Status FROM left LEFT JOIN right ON left.Id = right.Id");

        // Assert
        result.SelectColumns.Should().HaveCount(2);
        result.SelectColumns[1].SourceAlias.Should().Be("right");
        result.SelectColumns[1].SourceField.Should().Be("Status");
        result.SelectColumns[1].OutputField.Should().Be("Status");
        result.SelectColumns[1].DefaultValue.Should().Be("Unknown");
    }

    [Fact(DisplayName = "CsvJoinQueryParser Parse supports escaped single quote in coalesce default value.")]
    [Trait("Category", "Unit")]
    public void ParseSupportsEscapedSingleQuoteInCoalesceDefaultValue()
    {
        // Arrange
        var sut = new CsvJoinQueryParser();

        // Act
        var result = sut.Parse("SELECT COALESCE(right.Status, 'Can''t map') AS Status FROM left LEFT JOIN right ON left.Id = right.Id");

        // Assert
        result.SelectColumns[0].DefaultValue.Should().Be("Can't map");
    }

    [Fact(DisplayName = "CsvJoinQueryParser Parse throws for invalid query.")]
    [Trait("Category", "Unit")]
    public void ParseThrowsForInvalidQuery()
    {
        // Arrange
        var sut = new CsvJoinQueryParser();

        // Act
        Action action = () => _ = sut.Parse("SELECT broken");

        // Assert
        action.Should().Throw<FormatException>()
            .WithMessage("*Join query is invalid*");
    }

    [Fact(DisplayName = "CsvJoinQueryParser Parse throws when wildcard uses alias.")]
    [Trait("Category", "Unit")]
    public void ParseThrowsWhenWildcardUsesAlias()
    {
        // Arrange
        var sut = new CsvJoinQueryParser();

        // Act
        Action action = () => _ = sut.Parse("SELECT left.* AS Everything FROM left INNER JOIN right ON left.Id = right.Id");

        // Assert
        action.Should().Throw<FormatException>()
            .WithMessage("*Wildcard selections cannot use AS aliases*");
    }

    [Fact(DisplayName = "CsvJoinQueryParser Parse throws when join clause uses same alias on both sides.")]
    [Trait("Category", "Unit")]
    public void ParseThrowsWhenJoinClauseUsesSameAliasOnBothSides()
    {
        // Arrange
        var sut = new CsvJoinQueryParser();

        // Act
        Action action = () => _ = sut.Parse("SELECT left.Id FROM left INNER JOIN right ON left.Id = left.OtherId");

        // Assert
        action.Should().Throw<FormatException>()
            .WithMessage("*both source aliases*");
    }

    [Fact(DisplayName = "CsvJoinQueryParser Parse throws when join clause references unknown alias.")]
    [Trait("Category", "Unit")]
    public void ParseThrowsWhenJoinClauseReferencesUnknownAlias()
    {
        // Arrange
        var sut = new CsvJoinQueryParser();

        // Act
        Action action = () => _ = sut.Parse("SELECT left.Id FROM left INNER JOIN right ON third.Id = right.Id");

        // Assert
        action.Should().Throw<FormatException>()
            .WithMessage("*not declared*");
    }

    [Fact(DisplayName = "CsvJoinQueryParser Parse throws when wildcard is used in join clause.")]
    [Trait("Category", "Unit")]
    public void ParseThrowsWhenWildcardIsUsedInJoinClause()
    {
        // Arrange
        var sut = new CsvJoinQueryParser();

        // Act
        Action action = () => _ = sut.Parse("SELECT left.Id FROM left INNER JOIN right ON left.* = right.Id");

        // Assert
        action.Should().Throw<FormatException>()
            .WithMessage("*Wildcard is not allowed in JOIN ON clause*");
    }

    [Fact(DisplayName = "CsvJoinQueryParser Parse throws when select contains empty expression.")]
    [Trait("Category", "Unit")]
    public void ParseThrowsWhenSelectContainsEmptyExpression()
    {
        // Arrange
        var sut = new CsvJoinQueryParser();

        // Act
        Action action = () => _ = sut.Parse("SELECT left.Id, , right.Id FROM left INNER JOIN right ON left.Id = right.Id");

        // Assert
        action.Should().Throw<FormatException>()
            .WithMessage("*empty column expression*");
    }

    [Fact(DisplayName = "CsvJoinQueryParser Parse throws when coalesce uses wildcard.")]
    [Trait("Category", "Unit")]
    public void ParseThrowsWhenCoalesceUsesWildcard()
    {
        // Arrange
        var sut = new CsvJoinQueryParser();

        // Act
        Action action = () => _ = sut.Parse("SELECT COALESCE(right.*, 'Unknown') FROM left LEFT JOIN right ON left.Id = right.Id");

        // Assert
        action.Should().Throw<FormatException>()
            .WithMessage("*invalid*");
    }
}
