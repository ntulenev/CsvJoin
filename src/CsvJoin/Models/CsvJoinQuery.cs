namespace CsvJoin.Models;

internal sealed record CsvJoinQuery(
    string LeftAlias,
    string LeftJoinField,
    string RightAlias,
    string RightJoinField,
    JoinType JoinType,
    IReadOnlyList<SelectColumn> SelectColumns)
{
    public JoinSourceSide ResolveSide(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        if (string.Equals(alias, LeftAlias, StringComparison.OrdinalIgnoreCase))
        {
            return JoinSourceSide.Left;
        }

        if (string.Equals(alias, RightAlias, StringComparison.OrdinalIgnoreCase))
        {
            return JoinSourceSide.Right;
        }

        throw new InvalidOperationException($"SELECT clause references unknown source alias '{alias}'.");
    }

    public BoundJoinQuery Bind(CsvDataSet left, CsvDataSet right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var leftJoinHeader = left.ResolveHeader(LeftJoinField);
        var rightJoinHeader = right.ResolveHeader(RightJoinField);
        var boundColumns = new List<BoundSelectColumn>();

        foreach (var selectColumn in SelectColumns)
        {
            var sourceSide = ResolveSide(selectColumn.SourceAlias);
            var source = sourceSide == JoinSourceSide.Left ? left : right;
            boundColumns.AddRange(source.Bind(selectColumn, sourceSide));
        }

        return new BoundJoinQuery(JoinType, leftJoinHeader, rightJoinHeader, boundColumns);
    }
}
