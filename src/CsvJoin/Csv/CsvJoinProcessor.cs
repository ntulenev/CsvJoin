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
        var rows = BuildRows(boundQuery, left, right, joinKeys);

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
}
