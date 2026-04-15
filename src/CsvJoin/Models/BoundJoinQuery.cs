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
    public BoundJoinQuery(
        JoinType joinType,
        string leftJoinHeader,
        string rightJoinHeader,
        IReadOnlyList<BoundSelectColumn> selectColumns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(leftJoinHeader);
        ArgumentException.ThrowIfNullOrWhiteSpace(rightJoinHeader);
        ArgumentNullException.ThrowIfNull(selectColumns);

        JoinType = joinType;
        LeftJoinHeader = leftJoinHeader;
        RightJoinHeader = rightJoinHeader;
        SelectColumns = EnsureUniqueOutputNames(selectColumns);
        Headers = SelectColumns.Select(static column => column.OutputField).ToArray();
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
}
