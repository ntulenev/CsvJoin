using CsvJoin.Csv;
using CsvJoin.Models;

namespace CsvJoin.Tests.Csv;

public sealed class CsvJoinQueryParserTests
{
    [Fact]
    public void ParseWhenQueryIsValidReturnsParsedQuery()
    {
        var parser = new CsvJoinQueryParser();

        var result = parser.Parse(
            "SELECT left.Id, right.[Full Name] AS Name FROM left FULL JOIN right ON left.Id = right.ExternalId");

        Assert.Equal("left", result.LeftAlias);
        Assert.Equal("Id", result.LeftJoinField);
        Assert.Equal("right", result.RightAlias);
        Assert.Equal("ExternalId", result.RightJoinField);
        Assert.Equal(JoinType.Full, result.JoinType);
        Assert.Collection(
            result.SelectColumns,
            column =>
            {
                Assert.Equal("left", column.SourceAlias);
                Assert.Equal("Id", column.SourceField);
                Assert.Equal("Id", column.OutputField);
                Assert.False(column.IsWildcard);
            },
            column =>
            {
                Assert.Equal("right", column.SourceAlias);
                Assert.Equal("Full Name", column.SourceField);
                Assert.Equal("Name", column.OutputField);
                Assert.False(column.IsWildcard);
            });
    }

    [Fact]
    public void ParseWhenWildcardUsesAliasThrowsFormatException()
    {
        var parser = new CsvJoinQueryParser();

        var action = () => parser.Parse(
            "SELECT left.* AS Everything FROM left INNER JOIN right ON left.Id = right.Id");

        Assert.Throws<FormatException>(action);
    }
}
