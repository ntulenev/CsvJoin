using System.Text;
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
        var firstJoinReference = ParseFieldReference(match.Groups["leftRef"].Value, allowWildcard: false);
        var secondJoinReference = ParseFieldReference(match.Groups["rightRef"].Value, allowWildcard: false);

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
        var items = SplitCommaSeparated(selectSegment);
        var columns = new List<SelectColumn>(items.Count);

        foreach (var item in items)
        {
            var parts = SplitAlias(item);
            var fieldReference = ParseFieldReference(parts.Expression, allowWildcard: true);
            var outputField = parts.OutputAlias ?? fieldReference.SourceField;

            if (fieldReference.IsWildcard && parts.OutputAlias is not null)
            {
                throw new FormatException("Wildcard selections cannot use AS aliases.");
            }

            columns.Add(new SelectColumn(fieldReference.SourceAlias, fieldReference.SourceField, outputField, fieldReference.IsWildcard));
        }

        return columns;
    }

    private static (string Expression, string? OutputAlias) SplitAlias(string item)
    {
        var match = SelectAliasPattern().Match(item.Trim());
        if (!match.Success)
        {
            throw new FormatException($"SELECT expression '{item}' is invalid.");
        }

        var expression = match.Groups["expr"].Value.Trim();
        var aliasGroup = match.Groups["alias"];
        var outputAlias = aliasGroup.Success ? UnwrapIdentifier(aliasGroup.Value.Trim()) : null;
        return (expression, outputAlias);
    }

    private static FieldReference ParseFieldReference(string expression, bool allowWildcard)
    {
        var match = FieldPattern().Match(expression.Trim());
        if (!match.Success)
        {
            throw new FormatException($"Field reference '{expression}' is invalid. Use alias.Column or alias.[Column Name].");
        }

        var alias = match.Groups["alias"].Value;
        var fieldToken = match.Groups["field"].Value;
        if (fieldToken == "*")
        {
            if (!allowWildcard)
            {
                throw new FormatException("Wildcard is not allowed in JOIN ON clause.");
            }

            return new FieldReference(alias, fieldToken, true);
        }

        return new FieldReference(alias, UnwrapIdentifier(fieldToken), false);
    }

    private static List<string> SplitCommaSeparated(string input)
    {
        var parts = new List<string>();
        var current = new StringBuilder();
        var insideBracket = false;

        foreach (var character in input)
        {
            if (character == '[')
            {
                insideBracket = true;
            }
            else if (character == ']')
            {
                insideBracket = false;
            }
            else if (character == ',' && !insideBracket)
            {
                AddPart(parts, current);
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        AddPart(parts, current);
        return parts;
    }

    private static void AddPart(List<string> parts, StringBuilder current)
    {
        var value = current.ToString().Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new FormatException("SELECT clause contains an empty column expression.");
        }

        parts.Add(value);
    }

    private static string UnwrapIdentifier(string token)
    {
        var trimmed = token.Trim();
        if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
        {
            return trimmed[1..^1];
        }

        return trimmed;
    }

    [GeneratedRegex(
        "^SELECT\\s+(?<select>.+?)\\s+FROM\\s+(?<leftAlias>[A-Za-z_][A-Za-z0-9_]*)\\s+(?<joinType>INNER|LEFT|RIGHT|FULL)\\s+JOIN\\s+(?<rightAlias>[A-Za-z_][A-Za-z0-9_]*)\\s+ON\\s+(?<leftRef>.+?)\\s*=\\s*(?<rightRef>.+?)$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex QueryPattern();

    [GeneratedRegex(
        @"^(?<expr>.+?)(?:\s+AS\s+(?<alias>\[[^\]]+\]|[A-Za-z_][A-Za-z0-9_]*))?$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex SelectAliasPattern();

    [GeneratedRegex(
        @"^(?<alias>[A-Za-z_][A-Za-z0-9_]*)\.(?<field>\*|\[[^\]]+\]|[A-Za-z_][A-Za-z0-9_]*)$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex FieldPattern();

    private sealed record FieldReference(string SourceAlias, string SourceField, bool IsWildcard);
}
