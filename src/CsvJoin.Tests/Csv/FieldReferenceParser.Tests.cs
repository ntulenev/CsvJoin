using FluentAssertions;

using CsvJoin.Csv;

namespace CsvJoin.Tests.Csv;

public class FieldReferenceParserTests
{
    [Fact(DisplayName = "FieldReferenceParser Parse throws when wildcard is forbidden.")]
    [Trait("Category", "Unit")]
    public void ParseThrowsWhenWildcardIsForbidden()
    {
        // Arrange
        // Act
        Action action = () => _ = FieldReferenceParser.Parse("left.*", allowWildcard: false);

        // Assert
        action.Should().Throw<FormatException>();
    }
}
