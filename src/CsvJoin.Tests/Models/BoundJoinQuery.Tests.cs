using FluentAssertions;

using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class BoundJoinQueryTests
{
    [Fact(DisplayName = "BoundJoinQuery stores constructor values and exposes headers.")]
    [Trait("Category", "Unit")]
    public void BoundJoinQueryStoresConstructorValuesAndExposesHeaders()
    {
        // Arrange
        var columns = new[] { new BoundSelectColumn(JoinSourceSide.Left, "Id", "Id") };

        // Act
        var sut = new BoundJoinQuery(JoinType.Left, "Id", "ExternalId", columns);

        // Assert
        sut.JoinType.Should().Be(JoinType.Left);
        sut.LeftJoinHeader.Should().Be("Id");
        sut.RightJoinHeader.Should().Be("ExternalId");
        sut.SelectColumns.Should().HaveCount(1);
        sut.Headers.Should().Equal("Id");
    }

    [Fact(DisplayName = "BoundJoinQuery makes duplicate output names unique.")]
    [Trait("Category", "Unit")]
    public void BoundJoinQueryMakesDuplicateOutputNamesUnique()
    {
        // Arrange
        var columns = new[]
        {
            new BoundSelectColumn(JoinSourceSide.Left, "Id", "Id"),
            new BoundSelectColumn(JoinSourceSide.Right, "Id", "Id"),
        };

        // Act
        var sut = new BoundJoinQuery(JoinType.Inner, "Id", "Id", columns);

        // Assert
        sut.Headers.Should().Equal("Id", "right_Id_2");
    }
}
