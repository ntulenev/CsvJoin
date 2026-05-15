namespace CsvJoin.Models;

/// <summary>
/// Represents a join query that has been bound to concrete dataset headers.
/// </summary>
internal sealed class BoundJoinQuery
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BoundJoinQuery"/> class.
    /// </summary>
    /// <param name="joinType">The join type to execute.</param>
    /// <param name="leftJoinHeader">The concrete left join header.</param>
    /// <param name="rightJoinHeader">The concrete right join header.</param>
    /// <param name="selectColumns">The bound output columns.</param>
    /// <param name="sourceFilters">The bound source row filters applied before joining.</param>
    /// <param name="isDistinct">Indicates whether duplicate result rows should be removed.</param>
    /// <param name="orderByColumns">The output columns used to sort result rows.</param>
    /// <param name="limit">The maximum number of result rows to return.</param>
    public BoundJoinQuery(
        JoinType joinType,
        string leftJoinHeader,
        string rightJoinHeader,
        IReadOnlyList<BoundSelectColumn> selectColumns,
        IReadOnlyList<BoundSourceFilter>? sourceFilters = null,
        bool isDistinct = false,
        IReadOnlyList<OrderByColumn>? orderByColumns = null,
        int? limit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(leftJoinHeader);
        ArgumentException.ThrowIfNullOrWhiteSpace(rightJoinHeader);
        ArgumentNullException.ThrowIfNull(selectColumns);

        JoinType = joinType;
        LeftJoinHeader = leftJoinHeader;
        RightJoinHeader = rightJoinHeader;
        SelectColumns = EnsureUniqueOutputNames(selectColumns);
        Headers = SelectColumns.Select(static column => column.OutputField).ToArray();
        SourceFilters = sourceFilters ?? [];
        IsDistinct = isDistinct;
        OrderByColumns = BindOrderByColumns(orderByColumns ?? [], Headers);
        Limit = limit;
    }

    /// <summary>
    /// Gets the join type to execute.
    /// </summary>
    public JoinType JoinType { get; }

    /// <summary>
    /// Gets the bound join header for the left dataset.
    /// </summary>
    public string LeftJoinHeader { get; }

    /// <summary>
    /// Gets the bound join header for the right dataset.
    /// </summary>
    public string RightJoinHeader { get; }

    /// <summary>
    /// Gets the bound output columns.
    /// </summary>
    public IReadOnlyList<BoundSelectColumn> SelectColumns { get; }

    /// <summary>
    /// Gets the final output headers.
    /// </summary>
    public IReadOnlyList<string> Headers { get; }

    /// <summary>
    /// Gets bound source row filters applied before joining.
    /// </summary>
    public IReadOnlyList<BoundSourceFilter> SourceFilters { get; }

    /// <summary>
    /// Gets a value indicating whether duplicate result rows should be removed.
    /// </summary>
    public bool IsDistinct { get; }

    /// <summary>
    /// Gets resolved output columns used to sort result rows.
    /// </summary>
    public IReadOnlyList<BoundOrderByColumn> OrderByColumns { get; }

    /// <summary>
    /// Gets the maximum number of result rows to return.
    /// </summary>
    public int? Limit { get; }

    /// <summary>
    /// Projects an output row from the provided join rows.
    /// </summary>
    /// <param name="leftRow">The left join row, when available.</param>
    /// <param name="rightRow">The right join row, when available.</param>
    /// <returns>The projected output row.</returns>
    public IReadOnlyList<string?> ProjectRow(CsvDataRow? leftRow, CsvDataRow? rightRow) =>
        SelectColumns.Select(column => column.Project(leftRow, rightRow)).ToArray();

    private static BoundSelectColumn[] EnsureUniqueOutputNames(IReadOnlyList<BoundSelectColumn> columns)
    {
        var outputNameUsage = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        return columns
            .Select(column => column with { OutputField = BuildUniqueOutputName(column, outputNameUsage) })
            .ToArray();
    }

    private static string BuildUniqueOutputName(BoundSelectColumn column, Dictionary<string, int> outputNameUsage)
    {
        var baseName = string.IsNullOrWhiteSpace(column.OutputField)
            ? column.SourceField
            : column.OutputField;

        if (!outputNameUsage.TryGetValue(baseName, out var usage))
        {
            outputNameUsage[baseName] = 1;
            return baseName;
        }

        usage++;
        outputNameUsage[baseName] = usage;
        var sourcePrefix = column.SourceSide == JoinSourceSide.Left ? "left" : "right";
        return $"{sourcePrefix}_{baseName}_{usage}";
    }

    private static BoundOrderByColumn[] BindOrderByColumns(
        IReadOnlyList<OrderByColumn> orderByColumns,
        IReadOnlyList<string> headers)
    {
        var resolvedColumns = new List<BoundOrderByColumn>();

        foreach (var orderByColumn in orderByColumns)
        {
            var index = ResolveHeaderIndex(orderByColumn.OutputField, headers);
            resolvedColumns.Add(new BoundOrderByColumn(headers[index], index, orderByColumn.Direction));
        }

        return resolvedColumns.ToArray();
    }

    private static int ResolveHeaderIndex(string outputField, IReadOnlyList<string> headers)
    {
        for (var index = 0; index < headers.Count; index++)
        {
            if (string.Equals(headers[index], outputField, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        throw new InvalidOperationException($"ORDER BY column '{outputField}' was not found in result columns.");
    }
}
