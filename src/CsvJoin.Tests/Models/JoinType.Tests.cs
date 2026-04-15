using FluentAssertions;

using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class JoinTypeTests
{
    [Fact(DisplayName = "JoinType exposes expected values.")]
    [Trait("Category", "Unit")]
    public void JoinTypeExposesExpectedValues()
    {
        // Arrange
        var values = Enum.GetValues<JoinType>();
        var expectedValues = new[] { JoinType.Inner, JoinType.Left, JoinType.Right, JoinType.Full };

        // Act
        // Assert
        values.Should().Equal(expectedValues);
    }
}
