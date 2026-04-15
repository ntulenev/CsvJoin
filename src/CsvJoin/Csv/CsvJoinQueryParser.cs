using System.Text.RegularExpressions;

using CsvJoin.Abstractions.Csv;
using CsvJoin.Csv.Syntax;
using CsvJoin.Models;

namespace CsvJoin.Csv;

/// <summary>
/// Parses SQL-like join queries into <see cref="CsvJoinQuery"/> instances.
/// </summary>
internal sealed partial class CsvJoinQueryParser : ICsvJoinQueryParser
{
    /// <inheritdoc />
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

        var syntax = new JoinQuerySyntax(
            ParseSelectColumns(match.Groups["select"].Value),
            match.Groups["leftAlias"].Value,
            match.Groups["rightAlias"].Value,
            ParseJoinType(match.Groups["joinType"].Value),
            new JoinConditionSyntax(
                FieldReferenceParser.Parse(match.Groups["leftRef"].Value, allowWildcard: false),
                FieldReferenceParser.Parse(match.Groups["rightRef"].Value, allowWildcard: false)));

        return syntax.ToDomainQuery();
    }

    private static JoinType ParseJoinType(string rawJoinType) => rawJoinType.ToUpperInvariant() switch
    {
        "INNER" => JoinType.Inner,
        "LEFT" => JoinType.Left,
        "RIGHT" => JoinType.Right,
        "FULL" => JoinType.Full,
        _ => throw new FormatException($"Unsupported join type '{rawJoinType}'."),
    };

    private static SelectProjectionSyntax[] ParseSelectColumns(string selectSegment) =>
        SelectClauseTokenizer.Split(selectSegment)
            .Select(SelectProjectionParser.Parse)
            .ToArray();

    [GeneratedRegex(
        "^SELECT\\s+(?<select>.+?)\\s+FROM\\s+(?<leftAlias>[A-Za-z_][A-Za-z0-9_]*)\\s+(?<joinType>INNER|LEFT|RIGHT|FULL)\\s+JOIN\\s+(?<rightAlias>[A-Za-z_][A-Za-z0-9_]*)\\s+ON\\s+(?<leftRef>.+?)\\s*=\\s*(?<rightRef>.+?)$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex QueryPattern();
}
