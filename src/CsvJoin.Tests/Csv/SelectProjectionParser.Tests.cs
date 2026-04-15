using FluentAssertions;

using CsvJoin.Csv;

namespace CsvJoin.Tests.Csv;

public class SelectProjectionParserTests
{
    [Fact(DisplayName = "SelectProjectionParser Parse returns select column for coalesce expression.")]
    [Trait("Category", "Unit")]
    public void ParseReturnsSelectColumnForCoalesceExpression()
    {
        // Arrange
        const string input = "COALESCE(right.Status, 'Unknown') AS Status";

        // Act
        var result = SelectProjectionParser.Parse(input);

        // Assert
        result.SourceAlias.Should().Be("right");
        result.SourceField.Should().Be("Status");
        result.OutputField.Should().Be("Status");
        result.DefaultValue.Should().Be("Unknown");
    }

    [Fact(DisplayName = "SelectProjectionParser ParseFieldReference throws when wildcard is forbidden.")]
    [Trait("Category", "Unit")]
    public void ParseFieldReferenceThrowsWhenWildcardIsForbidden()
    {
        // Arrange
        // Act
        Action action = () => _ = SelectProjectionParser.ParseFieldReference("left.*", allowWildcard: false);

        // Assert
        action.Should().Throw<FormatException>();
    }
}
