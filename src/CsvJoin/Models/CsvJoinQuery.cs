namespace CsvJoin.Models;

/// <summary>
/// Represents a parsed SQL-like join query.
/// </summary>
/// <param name="LeftAlias">The left source alias.</param>
/// <param name="LeftJoinField">The left join field.</param>
/// <param name="RightAlias">The right source alias.</param>
/// <param name="RightJoinField">The right join field.</param>
/// <param name="JoinType">The join type.</param>
/// <param name="SelectColumns">The selected output columns.</param>
/// <param name="SourceFilters">The source row filters applied before joining.</param>
/// <param name="IsDistinct">Indicates whether duplicate result rows should be removed.</param>
/// <param name="OrderByColumns">The output columns used to sort result rows.</param>
/// <param name="Limit">The maximum number of result rows to return.</param>
internal sealed record CsvJoinQuery(
    string LeftAlias,
    string LeftJoinField,
    string RightAlias,
    string RightJoinField,
    JoinType JoinType,
    IReadOnlyList<SelectColumn> SelectColumns,
    IReadOnlyList<SourceFilter>? SourceFilters = null,
    bool IsDistinct = false,
    IReadOnlyList<OrderByColumn>? OrderByColumns = null,
    int? Limit = null)
{
    /// <summary>
    /// Resolves a query alias to a join side.
    /// </summary>
    /// <param name="alias">The alias to resolve.</param>
    /// <returns>The join side for the alias.</returns>
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

    /// <summary>
    /// Binds the query to concrete dataset headers.
    /// </summary>
    /// <param name="left">The left dataset.</param>
    /// <param name="right">The right dataset.</param>
    /// <returns>A bound join query.</returns>
    public BoundJoinQuery Bind(CsvDataSet left, CsvDataSet right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var leftJoinHeader = left.ResolveHeader(LeftJoinField);
        var rightJoinHeader = right.ResolveHeader(RightJoinField);
        var boundColumns = new List<BoundSelectColumn>();
        var boundFilters = new List<BoundSourceFilter>();

        foreach (var selectColumn in SelectColumns)
        {
            var sourceSide = ResolveSide(selectColumn.SourceAlias);
            var source = sourceSide == JoinSourceSide.Left ? left : right;
            boundColumns.AddRange(source.Bind(selectColumn, sourceSide));
        }

        foreach (var sourceFilter in SourceFilters ?? [])
        {
            var sourceSide = ResolveSide(sourceFilter.SourceAlias);
            var source = sourceSide == JoinSourceSide.Left ? left : right;
            boundFilters.Add(new BoundSourceFilter(
                sourceSide,
                source.ResolveHeader(sourceFilter.SourceField),
                sourceFilter.Operator,
                sourceFilter.Value));
        }

        return new BoundJoinQuery(
            JoinType,
            leftJoinHeader,
            rightJoinHeader,
            boundColumns,
            boundFilters,
            IsDistinct,
            OrderByColumns ?? [],
            Limit);
    }
}
