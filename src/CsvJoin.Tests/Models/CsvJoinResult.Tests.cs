using FluentAssertions;

using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class CsvJoinResultTests
{
    [Fact(DisplayName = "CsvJoinResult stores constructor values.")]
    [Trait("Category", "Unit")]
    public void CsvJoinResultStoresConstructorValues()
    {
        // Arrange
        var headers = new[] { "Id" };
        var rows = new[] { (IReadOnlyList<string?>)new string?[] { "1" } };

        // Act
        var sut = new CsvJoinResult("left.csv", "right.csv", headers, rows);

        // Assert
        sut.LeftFilePath.Should().Be("left.csv");
        sut.RightFilePath.Should().Be("right.csv");
        sut.Headers.Should().BeSameAs(headers);
        sut.Rows.Should().BeSameAs(rows);
    }
}
