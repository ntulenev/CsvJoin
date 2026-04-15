using CsvJoin.Csv;
using CsvJoin.Models;

namespace CsvJoin.Tests.Csv;

public sealed class CsvJoinProcessorTests
{
    [Fact]
    public void ProcessWhenLeftJoinReturnsUnmatchedLeftRows()
    {
        var processor = new CsvJoinProcessor();
        var query = new CsvJoinQuery(
            "left",
            "Id",
            "right",
            "Id",
            JoinType.Left,
            [
                new SelectColumn("left", "Id", "Id"),
                new SelectColumn("right", "Status", "Status"),
            ]);

        var left = BuildDataSet(
            "left",
            ["Id", "Name"],
            [["1", "Alpha"], ["2", "Beta"]]);
        var right = BuildDataSet(
            "right",
            ["Id", "Status"],
            [["1", "Ready"]]);

        var result = processor.Process(query, left, right);

        Assert.Equal(2, result.Rows.Count);
        var expectedFirstRow = new[] { "1", "Ready" };
        var expectedSecondRow = new string?[] { "2", null };

        Assert.Equal(expectedFirstRow, result.Rows[0]);
        Assert.Equal(expectedSecondRow, result.Rows[1]);
    }

    [Fact]
    public void ProcessWhenFullJoinReturnsUnmatchedRowsFromBothSources()
    {
        var processor = new CsvJoinProcessor();
        var query = new CsvJoinQuery(
            "left",
            "Id",
            "right",
            "Id",
            JoinType.Full,
            [
                new SelectColumn("left", "Id", "LeftId"),
                new SelectColumn("right", "Id", "RightId"),
            ]);

        var left = BuildDataSet(
            "left",
            ["Id"],
            [["1"], ["2"]]);
        var right = BuildDataSet(
            "right",
            ["Id"],
            [["2"], ["3"]]);

        var result = processor.Process(query, left, right);

        Assert.Equal(3, result.Rows.Count);
        var expectedFirstRow = new string?[] { "1", null };
        var expectedSecondRow = new string?[] { "2", "2" };
        var expectedThirdRow = new string?[] { null, "3" };

        Assert.Equal(expectedFirstRow, result.Rows[0]);
        Assert.Equal(expectedSecondRow, result.Rows[1]);
        Assert.Equal(expectedThirdRow, result.Rows[2]);
    }

    [Fact]
    public void ProcessWhenWildcardSelectionDuplicatesHeaderNamesMakesThemUnique()
    {
        var processor = new CsvJoinProcessor();
        var query = new CsvJoinQuery(
            "left",
            "Id",
            "right",
            "Id",
            JoinType.Inner,
            [
                new SelectColumn("left", "*", "*", true),
                new SelectColumn("right", "*", "*", true),
            ]);

        var left = BuildDataSet(
            "left",
            ["Id", "Value"],
            [["1", "L"]]);
        var right = BuildDataSet(
            "right",
            ["Id", "Value"],
            [["1", "R"]]);

        var result = processor.Process(query, left, right);

        var expectedHeaders = new[] { "Id", "Value", "right_Id_2", "right_Value_2" };

        Assert.Equal(expectedHeaders, result.Headers);
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
