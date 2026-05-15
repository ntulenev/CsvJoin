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
        var diagnostics = new JoinDiagnostics(
            LeftSourceRows: 2,
            RightSourceRows: 3,
            LeftRowsAfterFilters: 1,
            RightRowsAfterFilters: 2,
            MatchedRowPairs: 1,
            UnmatchedLeftRows: 0,
            UnmatchedRightRows: 1,
            ProjectedRowsBeforeResultOptions: 1,
            DuplicateLeftJoinKeys: 0,
            DuplicateRightJoinKeys: 0);

        // Act
        var sut = new CsvJoinResult("left.csv", "right.csv", headers, rows, diagnostics);

        // Assert
        sut.LeftFilePath.Should().Be("left.csv");
        sut.RightFilePath.Should().Be("right.csv");
        sut.Headers.Should().BeSameAs(headers);
        sut.Rows.Should().BeSameAs(rows);
        sut.Diagnostics.Should().BeSameAs(diagnostics);
    }
}
