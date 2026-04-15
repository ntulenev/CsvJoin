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

        var boundQuery = query.Bind(left, right);
        var rows = BuildRows(boundQuery, left, right);

        return new CsvJoinResult(left.FilePath, right.FilePath, boundQuery.Headers, rows);
    }

    private static List<IReadOnlyList<string?>> BuildRows(
        BoundJoinQuery query,
        CsvDataSet left,
        CsvDataSet right)
    {
        var rightLookup = right.BuildLookup(query.RightJoinHeader);
        var matchedRightIndexes = new HashSet<int>();
        var resultRows = new List<IReadOnlyList<string?>>();

        foreach (var leftRow in left.Rows)
        {
            var leftKey = leftRow.GetJoinKey(query.LeftJoinHeader);
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
