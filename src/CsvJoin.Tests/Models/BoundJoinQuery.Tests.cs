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
        var filters = new[] { new BoundSourceFilter(JoinSourceSide.Left, "Status", SourceFilterOperator.IsNotNull) };

        // Act
        var sut = new BoundJoinQuery(
            JoinType.Left,
            "Id",
            "ExternalId",
            columns,
            sourceFilters: filters,
            isDistinct: true,
            orderByColumns: [new OrderByColumn("Id", OrderByDirection.Descending)],
            limit: 5);

        // Assert
        sut.JoinType.Should().Be(JoinType.Left);
        sut.LeftJoinHeader.Should().Be("Id");
        sut.RightJoinHeader.Should().Be("ExternalId");
        sut.SelectColumns.Should().HaveCount(1);
        sut.Headers.Should().Equal("Id");
        sut.SourceFilters.Should().BeSameAs(filters);
        sut.IsDistinct.Should().BeTrue();
        sut.OrderByColumns.Should().ContainSingle();
        sut.OrderByColumns[0].Index.Should().Be(0);
        sut.OrderByColumns[0].Direction.Should().Be(OrderByDirection.Descending);
        sut.Limit.Should().Be(5);
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
