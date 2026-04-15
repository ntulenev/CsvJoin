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
}
