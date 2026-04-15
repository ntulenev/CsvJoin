using CsvJoin.Abstractions.Csv;
using CsvJoin.Models;

namespace CsvJoin.Csv;

internal sealed class CsvJoinProcessor : ICsvJoinProcessor
{
    public CsvJoinResult Process(CsvJoinQuery query, CsvDataSet left, CsvDataSet right)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var leftJoinHeader = left.ResolveHeader(query.LeftJoinField);
        var rightJoinHeader = right.ResolveHeader(query.RightJoinField);
        var outputColumns = BuildOutputColumns(query, left, right);
        var rows = BuildRows(query, left, right, leftJoinHeader, rightJoinHeader, outputColumns);

        return new CsvJoinResult(left.FilePath, right.FilePath, outputColumns.Select(static x => x.OutputField).ToArray(), rows);
    }

    private static List<IReadOnlyList<string?>> BuildRows(
        CsvJoinQuery query,
        CsvDataSet left,
        CsvDataSet right,
        string leftJoinHeader,
        string rightJoinHeader,
        IReadOnlyList<JoinOutputColumn> outputColumns)
    {
        var rightLookup = BuildLookup(right.Rows, rightJoinHeader);
        var matchedRightIndexes = new HashSet<int>();
        var resultRows = new List<IReadOnlyList<string?>>();

        foreach (var leftRow in left.Rows)
        {
            var leftKey = GetJoinKey(leftRow.Values, leftJoinHeader);
            if (rightLookup.TryGetValue(leftKey, out var matchingRightRows))
            {
                foreach (var rightRow in matchingRightRows)
                {
                    matchedRightIndexes.Add(rightRow.Index);
                    resultRows.Add(BuildOutputRow(query, leftRow, rightRow, outputColumns));
                }

                continue;
            }

            if (query.JoinType is JoinType.Left or JoinType.Full)
            {
                resultRows.Add(BuildOutputRow(query, leftRow, null, outputColumns));
            }
        }

        if (query.JoinType is JoinType.Right or JoinType.Full)
        {
            foreach (var rightRow in right.Rows)
            {
                if (matchedRightIndexes.Contains(rightRow.Index))
                {
                    continue;
                }

                resultRows.Add(BuildOutputRow(query, null, rightRow, outputColumns));
            }
        }

        return resultRows;
    }

    private static Dictionary<string, List<CsvDataRow>> BuildLookup(IReadOnlyList<CsvDataRow> rows, string joinHeader)
    {
        var lookup = new Dictionary<string, List<CsvDataRow>>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            var joinKey = GetJoinKey(row.Values, joinHeader);
            if (!lookup.TryGetValue(joinKey, out var bucket))
            {
                bucket = [];
                lookup[joinKey] = bucket;
            }

            bucket.Add(row);
        }

        return lookup;
    }

    private static string GetJoinKey(IReadOnlyDictionary<string, string?> rowValues, string joinHeader)
    {
        if (!rowValues.TryGetValue(joinHeader, out var value))
        {
            return string.Empty;
        }

        return value ?? string.Empty;
    }

    private static string?[] BuildOutputRow(
        CsvJoinQuery query,
        CsvDataRow? leftRow,
        CsvDataRow? rightRow,
        IReadOnlyList<JoinOutputColumn> outputColumns)
    {
        var values = new string?[outputColumns.Count];
        for (var index = 0; index < outputColumns.Count; index++)
        {
            var outputColumn = outputColumns[index];
            var sourceRow = string.Equals(outputColumn.SourceAlias, query.LeftAlias, StringComparison.OrdinalIgnoreCase)
                ? leftRow
                : string.Equals(outputColumn.SourceAlias, query.RightAlias, StringComparison.OrdinalIgnoreCase)
                    ? rightRow
                    : null;

            var projectedValue = sourceRow is not null && sourceRow.Values.TryGetValue(outputColumn.SourceField, out var fieldValue)
                ? fieldValue
                : null;

            values[index] = projectedValue ?? outputColumn.DefaultValue;
        }

        return values;
    }

    private static JoinOutputColumn[] BuildOutputColumns(CsvJoinQuery query, CsvDataSet left, CsvDataSet right)
    {
        var rawColumns = new List<JoinOutputColumn>();
        foreach (var selectedColumn in query.SelectColumns)
        {
            var source = ResolveSource(query, left, right, selectedColumn.SourceAlias);
            if (selectedColumn.IsWildcard)
            {
                foreach (var header in source.Headers)
                {
                    rawColumns.Add(new JoinOutputColumn(selectedColumn.SourceAlias, header, header));
                }

                continue;
            }

            var actualHeader = source.ResolveHeader(selectedColumn.SourceField);
            rawColumns.Add(new JoinOutputColumn(selectedColumn.SourceAlias, actualHeader, selectedColumn.OutputField, selectedColumn.DefaultValue));
        }

        var outputNameUsage = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        return rawColumns
            .Select(column => column with { OutputField = BuildUniqueOutputName(column, outputNameUsage) })
            .ToArray();
    }

    private static string BuildUniqueOutputName(JoinOutputColumn column, Dictionary<string, int> outputNameUsage)
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
        return $"{column.SourceAlias}_{baseName}_{usage}";
    }

    private static CsvDataSet ResolveSource(CsvJoinQuery query, CsvDataSet left, CsvDataSet right, string sourceAlias)
    {
        if (string.Equals(sourceAlias, query.LeftAlias, StringComparison.OrdinalIgnoreCase))
        {
            return left;
        }

        if (string.Equals(sourceAlias, query.RightAlias, StringComparison.OrdinalIgnoreCase))
        {
            return right;
        }

        throw new InvalidOperationException($"SELECT clause references unknown source alias '{sourceAlias}'.");
    }
}
