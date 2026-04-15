using FluentAssertions;

using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class JoinSourceSideTests
{
    [Fact(DisplayName = "JoinSourceSide defines left and right values.")]
    [Trait("Category", "Unit")]
    public void JoinSourceSideDefinesLeftAndRightValues()
    {
        // Arrange
        // Act
        var values = Enum.GetValues<JoinSourceSide>();

        // Assert
        values.Should().Equal(JoinSourceSide.Left, JoinSourceSide.Right);
    }
}
