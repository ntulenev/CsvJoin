using FluentAssertions;

using CsvJoin.Csv;

namespace CsvJoin.Tests.Csv;

public class SelectClauseTokenizerTests
{
    [Fact(DisplayName = "SelectClauseTokenizer Split keeps commas inside functions and strings.")]
    [Trait("Category", "Unit")]
    public void SplitKeepsCommasInsideFunctionsAndStrings()
    {
        // Arrange
        const string input = "left.Id, COALESCE(right.Status, 'Unknown, later') AS Status";

        // Act
        var result = SelectClauseTokenizer.Split(input);

        // Assert
        result.Should().HaveCount(2);
        result[1].Should().Be("COALESCE(right.Status, 'Unknown, later') AS Status");
    }

    [Fact(DisplayName = "SelectClauseTokenizer Split throws for empty projection.")]
    [Trait("Category", "Unit")]
    public void SplitThrowsForEmptyProjection()
    {
        // Arrange
        const string input = "left.Id, , right.Status";

        // Act
        Action action = () => _ = SelectClauseTokenizer.Split(input);

        // Assert
        action.Should().Throw<FormatException>();
    }
}
