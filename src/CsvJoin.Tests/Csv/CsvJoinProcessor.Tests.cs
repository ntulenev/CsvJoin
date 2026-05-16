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

    [Fact(DisplayName = "CsvJoinProcessor Process uses default value for unmatched right row when select column defines fallback.")]
    [Trait("Category", "Unit")]
    public void ProcessUsesDefaultValueForUnmatchedRightRowWhenSelectColumnDefinesFallback()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery(
            "left",
            "Id",
            "right",
            "Id",
            JoinType.Left,
            [new SelectColumn("left", "Id", "Id"), new SelectColumn("right", "Status", "Status", false, "Unknown")]);
        var left = BuildDataSet("left", ["Id"], [["1"], ["2"]]);
        var right = BuildDataSet("right", ["Id", "Status"], [["1", "Ready"]]);

        // Act
        var result = sut.Process(query, left, right);

        // Assert
        result.Rows.Should().HaveCount(2);
        result.Rows[0].Should().Equal("1", "Ready");
        result.Rows[1].Should().Equal("2", "Unknown");
    }

    [Fact(DisplayName = "CsvJoinProcessor Process uses default value for matched row when field value is null.")]
    [Trait("Category", "Unit")]
    public void ProcessUsesDefaultValueForMatchedRowWhenFieldValueIsNull()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery(
            "left",
            "Id",
            "right",
            "Id",
            JoinType.Left,
            [new SelectColumn("right", "Status", "Status", false, "Unknown")]);
        var left = BuildDataSet("left", ["Id"], [["1"]]);
        var right = BuildDataSetWithNullableValues("right", ["Id", "Status"], [["1", null]]);

        // Act
        var result = sut.Process(query, left, right);

        // Assert
        result.Rows.Should().ContainSingle();
        result.Rows[0].Should().Equal("Unknown");
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

    [Fact(DisplayName = "CsvJoinProcessor Process returns no rows for inner join without matches.")]
    [Trait("Category", "Unit")]
    public void ProcessReturnsNoRowsForInnerJoinWithoutMatches()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, [new SelectColumn("left", "Id", "Id")]);
        var left = BuildDataSet("left", ["Id"], [["1"]]);
        var right = BuildDataSet("right", ["Id"], [["2"]]);

        // Act
        var result = sut.Process(query, left, right);

        // Assert
        result.Rows.Should().BeEmpty();
    }

    [Fact(DisplayName = "CsvJoinProcessor Process returns unmatched right rows for right join.")]
    [Trait("Category", "Unit")]
    public void ProcessReturnsUnmatchedRightRowsForRightJoin()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Right, [new SelectColumn("left", "Id", "LeftId"), new SelectColumn("right", "Id", "RightId")]);
        var left = BuildDataSet("left", ["Id"], [["1"]]);
        var right = BuildDataSet("right", ["Id"], [["1"], ["2"]]);
        var expectedMatchedRow = new string?[] { "1", "1" };
        var expectedUnmatchedRow = new string?[] { null, "2" };

        // Act
        var result = sut.Process(query, left, right);

        // Assert
        result.Rows.Should().HaveCount(2);
        result.Rows[0].Should().Equal(expectedMatchedRow);
        result.Rows[1].Should().Equal(expectedUnmatchedRow);
    }

    [Fact(DisplayName = "CsvJoinProcessor Process creates cartesian matches for duplicate join keys.")]
    [Trait("Category", "Unit")]
    public void ProcessCreatesCartesianMatchesForDuplicateJoinKeys()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, [new SelectColumn("left", "Value", "LeftValue"), new SelectColumn("right", "Value", "RightValue")]);
        var left = BuildDataSet("left", ["Id", "Value"], [["1", "L1"], ["1", "L2"]]);
        var right = BuildDataSet("right", ["Id", "Value"], [["1", "R1"], ["1", "R2"]]);

        // Act
        var result = sut.Process(query, left, right);

        // Assert
        result.Rows.Should().HaveCount(4);
        result.Rows[0].Should().Equal("L1", "R1");
        result.Rows[1].Should().Equal("L1", "R2");
        result.Rows[2].Should().Equal("L2", "R1");
        result.Rows[3].Should().Equal("L2", "R2");
    }

    [Fact(DisplayName = "CsvJoinProcessor Process normalizes join keys when configured.")]
    [Trait("Category", "Unit")]
    public void ProcessNormalizesJoinKeysWhenConfigured()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery("left", "Code", "right", "Code", JoinType.Inner, [new SelectColumn("right", "Status", "Status")]);
        var left = BuildDataSet("left", ["Code"], [[" abc "]]);
        var right = BuildDataSet("right", ["Code", "Status"], [["ABC", "Ready"]]);
        var joinKeys = new JoinKeyNormalizationSettings(TrimWhitespace: true, IgnoreCase: true);

        // Act
        var result = sut.Process(query, left, right, joinKeys);

        // Assert
        result.Rows.Should().ContainSingle();
        result.Rows[0].Should().Equal("Ready");
    }

    [Fact(DisplayName = "CsvJoinProcessor Process applies distinct order by and limit.")]
    [Trait("Category", "Unit")]
    public void ProcessAppliesDistinctOrderByAndLimit()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery(
            "left",
            "Id",
            "right",
            "Id",
            JoinType.Inner,
            [new SelectColumn("left", "Name", "Name"), new SelectColumn("right", "Score", "Score")],
            IsDistinct: true,
            OrderByColumns: [new OrderByColumn("Score", OrderByDirection.Descending), new OrderByColumn("Name", OrderByDirection.Ascending)],
            Limit: 2);
        var left = BuildDataSet("left", ["Id", "Name"], [["1", "Beta"], ["2", "Alpha"], ["3", "Beta"]]);
        var right = BuildDataSet("right", ["Id", "Score"], [["1", "10"], ["2", "20"], ["3", "10"]]);

        // Act
        var result = sut.Process(query, left, right);

        // Assert
        result.Rows.Should().HaveCount(2);
        result.Rows[0].Should().Equal("Alpha", "20");
        result.Rows[1].Should().Equal("Beta", "10");
    }

    [Fact(DisplayName = "CsvJoinProcessor Process applies source filters before joining.")]
    [Trait("Category", "Unit")]
    public void ProcessAppliesSourceFiltersBeforeJoining()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery(
            "left",
            "Id",
            "right",
            "Id",
            JoinType.Left,
            [new SelectColumn("left", "Id", "Id"), new SelectColumn("right", "Status", "Status", false, "Unknown")],
            SourceFilters:
            [
                new SourceFilter("left", "Name", SourceFilterOperator.NotEquals, "Beta"),
                new SourceFilter("right", "Status", SourceFilterOperator.IsNotNull),
            ]);
        var left = BuildDataSet("left", ["Id", "Name"], [["1", "Alpha"], ["2", "Beta"], ["3", "Gamma"], ["4", "Delta"]]);
        var right = BuildDataSetWithNullableValues("right", ["Id", "Status"], [["1", "Ready"], ["2", "Ready"], ["3", null], ["4", "Done"]]);

        // Act
        var result = sut.Process(query, left, right);

        // Assert
        result.Rows.Should().HaveCount(3);
        result.Rows[0].Should().Equal("1", "Ready");
        result.Rows[1].Should().Equal("3", "Unknown");
        result.Rows[2].Should().Equal("4", "Done");
        result.Diagnostics.Should().NotBeNull();
        result.Diagnostics!.LeftSourceRows.Should().Be(4);
        result.Diagnostics.RightSourceRows.Should().Be(4);
        result.Diagnostics.LeftRowsAfterFilters.Should().Be(3);
        result.Diagnostics.RightRowsAfterFilters.Should().Be(3);
        result.Diagnostics.MatchedRowPairs.Should().Be(2);
        result.Diagnostics.UnmatchedLeftRows.Should().Be(1);
        result.Diagnostics.UnmatchedRightRows.Should().Be(1);
        result.Diagnostics.ProjectedRowsBeforeResultOptions.Should().Be(3);
        result.Diagnostics.DuplicateLeftJoinKeys.Should().Be(0);
        result.Diagnostics.DuplicateRightJoinKeys.Should().Be(0);
    }

    [Fact(DisplayName = "CsvJoinProcessor Process applies extended source filters.")]
    [Trait("Category", "Unit")]
    public void ProcessAppliesExtendedSourceFilters()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery(
            "left",
            "Id",
            "right",
            "Id",
            JoinType.Inner,
            [new SelectColumn("left", "Name", "Name"), new SelectColumn("right", "Score", "Score")],
            SourceFilters:
            [
                new SourceFilter("left", "Name", SourceFilterOperator.Contains, "li"),
                new SourceFilter("right", "Status", SourceFilterOperator.In, Values: ["Active", "Blocked"]),
                new SourceFilter("right", "Score", SourceFilterOperator.GreaterThanOrEqual, "90"),
            ]);
        var left = BuildDataSet("left", ["Id", "Name"], [["1", "Alice"], ["2", "Bob"], ["3", "Lidia"]]);
        var right = BuildDataSet("right", ["Id", "Status", "Score"], [["1", "Active", "95"], ["2", "Active", "99"], ["3", "Blocked", "80"]]);

        // Act
        var result = sut.Process(query, left, right);

        // Assert
        result.Rows.Should().ContainSingle();
        result.Rows[0].Should().Equal("Alice", "95");
    }

    [Fact(DisplayName = "CsvJoinProcessor Process throws when order by column is missing.")]
    [Trait("Category", "Unit")]
    public void ProcessThrowsWhenOrderByColumnIsMissing()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery(
            "left",
            "Id",
            "right",
            "Id",
            JoinType.Inner,
            [new SelectColumn("left", "Name", "Name")],
            OrderByColumns: [new OrderByColumn("Score", OrderByDirection.Ascending)]);
        var left = BuildDataSet("left", ["Id", "Name"], [["1", "Alpha"]]);
        var right = BuildDataSet("right", ["Id", "Score"], [["1", "20"]]);

        // Act
        Action action = () => _ = sut.Process(query, left, right);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*ORDER BY column 'Score'*");
    }

    [Fact(DisplayName = "CsvJoinProcessor Process throws when selected column is missing.")]
    [Trait("Category", "Unit")]
    public void ProcessThrowsWhenSelectedColumnIsMissing()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, [new SelectColumn("left", "Missing", "Missing")]);
        var left = BuildDataSet("left", ["Id"], [["1"]]);
        var right = BuildDataSet("right", ["Id"], [["1"]]);

        // Act
        Action action = () => _ = sut.Process(query, left, right);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Missing*");
    }

    [Fact(DisplayName = "CsvJoinProcessor Process throws when selected source alias is unknown.")]
    [Trait("Category", "Unit")]
    public void ProcessThrowsWhenSelectedSourceAliasIsUnknown()
    {
        // Arrange
        var sut = new CsvJoinProcessor();
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, [new SelectColumn("third", "Id", "Id")]);
        var left = BuildDataSet("left", ["Id"], [["1"]]);
        var right = BuildDataSet("right", ["Id"], [["1"]]);

        // Act
        Action action = () => _ = sut.Process(query, left, right);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*unknown source alias*");
    }

    private static CsvDataSet BuildDataSet(string alias, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var nullableRows = rows
            .Select(values => values.Select(static value => (string?)value).ToArray())
            .ToArray();

        return BuildDataSetWithNullableValues(alias, headers, nullableRows);
    }

    private static CsvDataSet BuildDataSetWithNullableValues(string alias, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string?>> rows)
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
