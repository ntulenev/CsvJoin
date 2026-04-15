using FluentAssertions;

using CsvJoin.Csv;
using CsvJoin.Models;

namespace CsvJoin.Tests.Csv;

public class CsvJoinProcessorTests
{
    [Fact(DisplayName = "CsvJoinProcessor Process throws when query is null.")]
    [Trait("Category", "Unit")]
    public void ProcessThrowsWhenQueryIsNull()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, [new SelectColumn("left", "Id", "Id")]);
        var dataSet = BuildDataSet("left", ["Id"], [["1"]]);

        // Act
        Action action = () => _ = sut.Process(null!, dataSet, dataSet);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "CsvJoinProcessor Process throws when left dataset is null.")]
    [Trait("Category", "Unit")]
    public void ProcessThrowsWhenLeftDatasetIsNull()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, [new SelectColumn("left", "Id", "Id")]);
        var dataSet = BuildDataSet("left", ["Id"], [["1"]]);

        // Act
        Action action = () => _ = sut.Process(query, null!, dataSet);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "CsvJoinProcessor Process throws when right dataset is null.")]
    [Trait("Category", "Unit")]
    public void ProcessThrowsWhenRightDatasetIsNull()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, [new SelectColumn("left", "Id", "Id")]);
        var dataSet = BuildDataSet("left", ["Id"], [["1"]]);

        // Act
        Action action = () => _ = sut.Process(query, dataSet, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "CsvJoinProcessor Process returns unmatched left rows for left join.")]
    [Trait("Category", "Unit")]
    public void ProcessReturnsUnmatchedLeftRowsForLeftJoin()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Left, [new SelectColumn("left", "Id", "Id"), new SelectColumn("right", "Status", "Status")]);
        var left = BuildDataSet("left", ["Id", "Name"], [["1", "Alpha"], ["2", "Beta"]]);
        var right = BuildDataSet("right", ["Id", "Status"], [["1", "Ready"]]);
        var expectedFirstRow = new[] { "1", "Ready" };
        var expectedSecondRow = new string?[] { "2", null };

        // Act
        var result = sut.Process(query, left, right);

        // Assert
        result.Rows.Should().HaveCount(2);
        result.Rows[0].Should().Equal(expectedFirstRow);
        result.Rows[1].Should().Equal(expectedSecondRow);
    }

    [Fact(DisplayName = "CsvJoinProcessor Process returns unmatched rows from both sides for full join.")]
    [Trait("Category", "Unit")]
    public void ProcessReturnsUnmatchedRowsFromBothSidesForFullJoin()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Full, [new SelectColumn("left", "Id", "LeftId"), new SelectColumn("right", "Id", "RightId")]);
        var left = BuildDataSet("left", ["Id"], [["1"], ["2"]]);
        var right = BuildDataSet("right", ["Id"], [["2"], ["3"]]);
        var expectedFirstRow = new string?[] { "1", null };
        var expectedSecondRow = new string?[] { "2", "2" };
        var expectedThirdRow = new string?[] { null, "3" };

        // Act
        var result = sut.Process(query, left, right);

        // Assert
        result.Rows.Should().HaveCount(3);
        result.Rows[0].Should().Equal(expectedFirstRow);
        result.Rows[1].Should().Equal(expectedSecondRow);
        result.Rows[2].Should().Equal(expectedThirdRow);
    }

    [Fact(DisplayName = "CsvJoinProcessor Process makes wildcard headers unique.")]
    [Trait("Category", "Unit")]
    public void ProcessMakesWildcardHeadersUnique()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, [new SelectColumn("left", "*", "*", true), new SelectColumn("right", "*", "*", true)]);
        var left = BuildDataSet("left", ["Id", "Value"], [["1", "L"]]);
        var right = BuildDataSet("right", ["Id", "Value"], [["1", "R"]]);
        var expectedHeaders = new[] { "Id", "Value", "right_Id_2", "right_Value_2" };

        // Act
        var result = sut.Process(query, left, right);

        // Assert
        result.Headers.Should().Equal(expectedHeaders);
    }

    private static CsvDataSet BuildDataSet(string alias, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var mappedRows = rows
            .Select((values, index) =>
            {
                var row = headers
                    .Select((header, headerIndex) => new KeyValuePair<string, string?>(header, values[headerIndex]))
                    .ToDictionary(static item => item.Key, static item => item.Value, StringComparer.OrdinalIgnoreCase);

                return new CsvDataRow(index, row);
            })
            .ToArray();

        return new CsvDataSet(alias, $"{alias}.csv", headers, mappedRows);
    }
}
