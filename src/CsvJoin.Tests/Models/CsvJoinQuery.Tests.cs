using FluentAssertions;

using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class CsvJoinQueryTests
{
    [Fact(DisplayName = "CsvJoinQuery stores constructor values.")]
    [Trait("Category", "Unit")]
    public void CsvJoinQueryStoresConstructorValues()
    {
        // Arrange
        var columns = new[] { new SelectColumn("left", "Id", "Id") };

        // Act
        var sut = new CsvJoinQuery("left", "Id", "right", "ExternalId", JoinType.Full, columns);

        // Assert
        sut.LeftAlias.Should().Be("left");
        sut.LeftJoinField.Should().Be("Id");
        sut.RightAlias.Should().Be("right");
        sut.RightJoinField.Should().Be("ExternalId");
        sut.JoinType.Should().Be(JoinType.Full);
        sut.SelectColumns.Should().BeSameAs(columns);
    }

    [Fact(DisplayName = "CsvJoinQuery ResolveSide returns left for left alias.")]
    [Trait("Category", "Unit")]
    public void ResolveSideReturnsLeftForLeftAlias()
    {
        // Arrange
        var sut = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, []);

        // Act
        var result = sut.ResolveSide("left");

        // Assert
        result.Should().Be(JoinSourceSide.Left);
    }

    [Fact(DisplayName = "CsvJoinQuery ResolveSide throws for unknown alias.")]
    [Trait("Category", "Unit")]
    public void ResolveSideThrowsForUnknownAlias()
    {
        // Arrange
        var sut = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, []);

        // Act
        Action action = () => _ = sut.ResolveSide("third");

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "CsvJoinQuery Bind creates bound join query.")]
    [Trait("Category", "Unit")]
    public void BindCreatesBoundJoinQuery()
    {
        // Arrange
        var sut = new CsvJoinQuery(
            "left",
            "Id",
            "right",
            "ExternalId",
            JoinType.Left,
            [new SelectColumn("left", "Id", "Id"), new SelectColumn("right", "Status", "Status")]);
        var left = new CsvDataSet("left", "left.csv", ["Id"], []);
        var right = new CsvDataSet("right", "right.csv", ["ExternalId", "Status"], []);

        // Act
        var result = sut.Bind(left, right);

        // Assert
        result.LeftJoinHeader.Should().Be("Id");
        result.RightJoinHeader.Should().Be("ExternalId");
        result.Headers.Should().Equal("Id", "Status");
    }
}
