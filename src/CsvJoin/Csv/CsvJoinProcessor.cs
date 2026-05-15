using CsvJoin.Abstractions.Csv;
using CsvJoin.Models;

namespace CsvJoin.Csv;

/// <summary>
/// Executes join queries against loaded CSV datasets.
/// </summary>
internal sealed class CsvJoinProcessor : ICsvJoinProcessor
{
    /// <inheritdoc />
    public CsvJoinResult Process(CsvJoinQuery query, CsvDataSet left, CsvDataSet right) =>
        Process(query, left, right, new JoinKeyNormalizationSettings(false, false));

    /// <summary>
    /// Executes join queries against loaded CSV datasets using configured join key normalization.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="left">The left dataset.</param>
    /// <param name="right">The right dataset.</param>
    /// <param name="joinKeys">The join key normalization settings.</param>
    /// <returns>The join result.</returns>
    public CsvJoinResult Process(
        CsvJoinQuery query,
        CsvDataSet left,
        CsvDataSet right,
        JoinKeyNormalizationSettings joinKeys)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(joinKeys);

        var boundQuery = query.Bind(left, right);
        var rows = ApplyResultOptions(BuildRows(boundQuery, left, right, joinKeys), boundQuery);

        return new CsvJoinResult(left.FilePath, right.FilePath, boundQuery.Headers, rows);
    }

    private static List<IReadOnlyList<string?>> BuildRows(
        BoundJoinQuery query,
        CsvDataSet left,
        CsvDataSet right,
        JoinKeyNormalizationSettings joinKeys)
    {
        var rightLookup = right.BuildLookup(query.RightJoinHeader, joinKeys);
        var matchedRightIndexes = new HashSet<int>();
        var resultRows = new List<IReadOnlyList<string?>>();

        foreach (var leftRow in left.Rows)
        {
            var leftKey = leftRow.GetJoinKey(query.LeftJoinHeader, joinKeys);
            if (rightLookup.TryGetValue(leftKey, out var matchingRightRows))
            {
                foreach (var rightRow in matchingRightRows)
                {
                    matchedRightIndexes.Add(rightRow.Index);
                    resultRows.Add(query.ProjectRow(leftRow, rightRow));
                }

                continue;
            }

            if (query.JoinType is JoinType.Left or JoinType.Full)
            {
                resultRows.Add(query.ProjectRow(leftRow, null));
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

                resultRows.Add(query.ProjectRow(null, rightRow));
            }
        }

        return resultRows;
    }

    private static IReadOnlyList<string?>[] ApplyResultOptions(
        IReadOnlyList<IReadOnlyList<string?>> rows,
        BoundJoinQuery query)
    {
        IEnumerable<IReadOnlyList<string?>> resultRows = rows;

        if (query.IsDistinct)
        {
            resultRows = resultRows.Distinct(RowValueComparer.Instance);
        }

        resultRows = ApplyOrdering(resultRows, query.OrderByColumns);

        if (query.Limit is not null)
        {
            resultRows = resultRows.Take(query.Limit.Value);
        }

        return resultRows.ToArray();
    }

    private static IEnumerable<IReadOnlyList<string?>> ApplyOrdering(
        IEnumerable<IReadOnlyList<string?>> rows,
        IReadOnlyList<BoundOrderByColumn> orderByColumns)
    {
        IOrderedEnumerable<IReadOnlyList<string?>>? orderedRows = null;

        foreach (var orderByColumn in orderByColumns)
        {
            orderedRows = ApplyOrderingColumn(orderedRows ?? rows, orderedRows is null, orderByColumn);
        }

        return orderedRows ?? rows;
    }

    private static IOrderedEnumerable<IReadOnlyList<string?>> ApplyOrderingColumn(
        IEnumerable<IReadOnlyList<string?>> rows,
        bool isFirstColumn,
        BoundOrderByColumn orderByColumn)
    {
        return orderByColumn.Direction == OrderByDirection.Descending
            ? ApplyDescendingOrdering(rows, isFirstColumn, orderByColumn.Index)
            : ApplyAscendingOrdering(rows, isFirstColumn, orderByColumn.Index);
    }

    private static IOrderedEnumerable<IReadOnlyList<string?>> ApplyAscendingOrdering(
        IEnumerable<IReadOnlyList<string?>> rows,
        bool isFirstColumn,
        int columnIndex)
    {
        return isFirstColumn
            ? rows.OrderBy(row => row[columnIndex], StringComparer.Ordinal)
            : ((IOrderedEnumerable<IReadOnlyList<string?>>)rows).ThenBy(row => row[columnIndex], StringComparer.Ordinal);
    }

    private static IOrderedEnumerable<IReadOnlyList<string?>> ApplyDescendingOrdering(
        IEnumerable<IReadOnlyList<string?>> rows,
        bool isFirstColumn,
        int columnIndex)
    {
        return isFirstColumn
            ? rows.OrderByDescending(row => row[columnIndex], StringComparer.Ordinal)
            : ((IOrderedEnumerable<IReadOnlyList<string?>>)rows).ThenByDescending(row => row[columnIndex], StringComparer.Ordinal);
    }

    private sealed class RowValueComparer : IEqualityComparer<IReadOnlyList<string?>>
    {
        public static RowValueComparer Instance { get; } = new();

        public bool Equals(IReadOnlyList<string?>? x, IReadOnlyList<string?>? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null || x.Count != y.Count)
            {
                return false;
            }

            for (var index = 0; index < x.Count; index++)
            {
                if (!string.Equals(x[index], y[index], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(IReadOnlyList<string?> obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            var hashCode = new HashCode();
            foreach (var value in obj)
            {
                hashCode.Add(value, StringComparer.Ordinal);
            }

            return hashCode.ToHashCode();
        }
    }
}
