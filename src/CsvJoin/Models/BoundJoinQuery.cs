namespace CsvJoin.Models;

internal sealed class BoundJoinQuery
{
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

    public JoinType JoinType { get; }

    public string LeftJoinHeader { get; }

    public string RightJoinHeader { get; }

    public IReadOnlyList<BoundSelectColumn> SelectColumns { get; }

    public IReadOnlyList<string> Headers { get; }

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
