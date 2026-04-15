using System.Text.RegularExpressions;

using CsvJoin.Models;

namespace CsvJoin.Csv;

internal static partial class SelectProjectionParser
{
    private const string CoalesceFunctionName = "COALESCE";

    public static SelectColumn Parse(string item)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(item);

        var parts = SplitAlias(item);
        var projection = ParseProjection(parts.Expression);
        var outputField = parts.OutputAlias ?? projection.SourceField;

        if (projection.IsWildcard && parts.OutputAlias is not null)
        {
            throw new FormatException("Wildcard selections cannot use AS aliases.");
        }

        return new SelectColumn(
            projection.SourceAlias,
            projection.SourceField,
            outputField,
            projection.IsWildcard,
            projection.DefaultValue);
    }

    public static (string SourceAlias, string SourceField, bool IsWildcard) ParseFieldReference(string expression, bool allowWildcard)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);

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

            return (alias, fieldToken, true);
        }

        return (alias, UnwrapIdentifier(fieldToken), false);
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

    private static ParsedProjection ParseProjection(string expression)
    {
        var trimmedExpression = expression.Trim();
        if (!trimmedExpression.StartsWith($"{CoalesceFunctionName}(", StringComparison.OrdinalIgnoreCase))
        {
            var fieldReference = ParseFieldReference(trimmedExpression, allowWildcard: true);
            return new ParsedProjection(fieldReference.SourceAlias, fieldReference.SourceField, fieldReference.IsWildcard, null);
        }

        var match = CoalescePattern().Match(trimmedExpression);
        if (!match.Success)
        {
            throw new FormatException($"SELECT expression '{expression}' is invalid.");
        }

        var parsedFieldReference = ParseFieldReference(match.Groups["field"].Value, allowWildcard: false);
        var defaultValue = UnescapeStringLiteral(match.Groups["default"].Value);
        return new ParsedProjection(parsedFieldReference.SourceAlias, parsedFieldReference.SourceField, parsedFieldReference.IsWildcard, defaultValue);
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

    private static string UnescapeStringLiteral(string token) => token.Replace("''", "'", StringComparison.Ordinal);

    [GeneratedRegex(
        @"^(?<expr>.+?)(?:\s+AS\s+(?<alias>\[[^\]]+\]|[A-Za-z_][A-Za-z0-9_]*))?$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex SelectAliasPattern();

    [GeneratedRegex(
        @"^COALESCE\s*\(\s*(?<field>[A-Za-z_][A-Za-z0-9_]*\.(?:\[[^\]]+\]|[A-Za-z_][A-Za-z0-9_]*))\s*,\s*'(?<default>(?:[^']|'')*)'\s*\)$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex CoalescePattern();

    [GeneratedRegex(
        @"^(?<alias>[A-Za-z_][A-Za-z0-9_]*)\.(?<field>\*|\[[^\]]+\]|[A-Za-z_][A-Za-z0-9_]*)$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex FieldPattern();

    private sealed record ParsedProjection(string SourceAlias, string SourceField, bool IsWildcard, string? DefaultValue);
}
