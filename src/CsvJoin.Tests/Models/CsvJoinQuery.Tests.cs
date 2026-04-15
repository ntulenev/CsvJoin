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
}
