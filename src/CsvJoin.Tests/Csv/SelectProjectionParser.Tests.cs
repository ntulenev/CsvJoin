using FluentAssertions;

using CsvJoin.Csv;
using CsvJoin.Csv.Syntax;

namespace CsvJoin.Tests.Csv;

public class SelectProjectionParserTests
{
    [Fact(DisplayName = "SelectProjectionParser Parse returns coalesce projection syntax for coalesce expression.")]
    [Trait("Category", "Unit")]
    public void ParseReturnsCoalesceProjectionSyntaxForCoalesceExpression()
    {
        // Arrange
        const string input = "COALESCE(right.Status, 'Unknown') AS Status";

        // Act
        var result = SelectProjectionParser.Parse(input);

        // Assert
        result.Should().BeOfType<CoalesceSelectProjectionSyntax>();
        result.FieldReference.SourceAlias.Should().Be("right");
        result.FieldReference.SourceField.Should().Be("Status");
        result.OutputField.Should().Be("Status");
        ((CoalesceSelectProjectionSyntax)result).DefaultValue.Should().Be("Unknown");
    }
}
