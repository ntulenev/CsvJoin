using System.Text.RegularExpressions;

using CsvJoin.Abstractions.Csv;
using CsvJoin.Models;

namespace CsvJoin.Csv;

internal sealed partial class CsvJoinQueryParser : ICsvJoinQueryParser
{
    private static readonly StringComparer AliasComparer = StringComparer.OrdinalIgnoreCase;

    public CsvJoinQuery Parse(string queryText)
    {
        if (string.IsNullOrWhiteSpace(queryText))
        {
            throw new FormatException("Join query must not be empty.");
        }

        var match = QueryPattern().Match(queryText.Trim());
        if (!match.Success)
        {
            throw new FormatException(
                "Join query is invalid. Expected format: SELECT left.Id, right.Name FROM left INNER JOIN right ON left.Id = right.Id");
        }

        var selectSegment = match.Groups["select"].Value;
        var leftAlias = match.Groups["leftAlias"].Value;
        var rightAlias = match.Groups["rightAlias"].Value;
        var joinType = ParseJoinType(match.Groups["joinType"].Value);
        var firstJoinReference = SelectProjectionParser.ParseFieldReference(match.Groups["leftRef"].Value, allowWildcard: false);
        var secondJoinReference = SelectProjectionParser.ParseFieldReference(match.Groups["rightRef"].Value, allowWildcard: false);

        if (AliasComparer.Equals(firstJoinReference.SourceAlias, secondJoinReference.SourceAlias))
        {
            throw new FormatException("JOIN ON clause must reference both source aliases.");
        }

        var aliases = new HashSet<string>(AliasComparer) { leftAlias, rightAlias };
        if (!aliases.Contains(firstJoinReference.SourceAlias) || !aliases.Contains(secondJoinReference.SourceAlias))
        {
            throw new FormatException("JOIN ON clause references an alias that is not declared in FROM/JOIN.");
        }

        var parsedColumns = ParseSelectColumns(selectSegment);
        if (parsedColumns.Count == 0)
        {
            throw new FormatException("SELECT clause must contain at least one column.");
        }

        var leftJoinField = AliasComparer.Equals(firstJoinReference.SourceAlias, leftAlias)
            ? firstJoinReference.SourceField
            : secondJoinReference.SourceField;

        var rightJoinField = AliasComparer.Equals(firstJoinReference.SourceAlias, rightAlias)
            ? firstJoinReference.SourceField
            : secondJoinReference.SourceField;

        return new CsvJoinQuery(leftAlias, leftJoinField, rightAlias, rightJoinField, joinType, parsedColumns);
    }

    private static JoinType ParseJoinType(string rawJoinType) => rawJoinType.ToUpperInvariant() switch
    {
        "INNER" => JoinType.Inner,
        "LEFT" => JoinType.Left,
        "RIGHT" => JoinType.Right,
        "FULL" => JoinType.Full,
        _ => throw new FormatException($"Unsupported join type '{rawJoinType}'."),
    };

    private static List<SelectColumn> ParseSelectColumns(string selectSegment)
    {
        return SelectClauseTokenizer.Split(selectSegment)
            .Select(SelectProjectionParser.Parse)
            .ToList();
    }

    [GeneratedRegex(
        "^SELECT\\s+(?<select>.+?)\\s+FROM\\s+(?<leftAlias>[A-Za-z_][A-Za-z0-9_]*)\\s+(?<joinType>INNER|LEFT|RIGHT|FULL)\\s+JOIN\\s+(?<rightAlias>[A-Za-z_][A-Za-z0-9_]*)\\s+ON\\s+(?<leftRef>.+?)\\s*=\\s*(?<rightRef>.+?)$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex QueryPattern();

}
