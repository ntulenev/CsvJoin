using System.Text.RegularExpressions;

using CsvJoin.Csv.Syntax;

namespace CsvJoin.Csv;

/// <summary>
/// Parses individual SELECT projections.
/// </summary>
internal static partial class SelectProjectionParser
{
    /// <summary>
    /// Parses a single projection expression.
    /// </summary>
    /// <param name="item">The projection text.</param>
    /// <returns>The parsed projection syntax node.</returns>
    public static SelectProjectionSyntax Parse(string item)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(item);

        var parts = SplitAlias(item);
        var projection = ParseProjection(parts.Expression);
        var outputField = parts.OutputAlias ?? projection.FieldReference.SourceField;

        if (projection.FieldReference.IsWildcard && parts.OutputAlias is not null)
        {
            throw new FormatException("Wildcard selections cannot use AS aliases.");
        }

        return projection switch
        {
            CoalesceSelectProjectionSyntax coalesceProjection => coalesceProjection with { OutputField = outputField },
            FieldSelectProjectionSyntax fieldProjection => fieldProjection with { OutputField = outputField },
            _ => throw new InvalidOperationException($"Unsupported projection type '{projection.GetType().Name}'."),
        };
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
        var outputAlias = aliasGroup.Success ? FieldReferenceParser.UnwrapIdentifier(aliasGroup.Value.Trim()) : null;
        return (expression, outputAlias);
    }

    private static SelectProjectionSyntax ParseProjection(string expression)
    {
        var trimmedExpression = expression.Trim();
        if (!trimmedExpression.StartsWith($"{CoalesceFunctionName}(", StringComparison.OrdinalIgnoreCase))
        {
            return new FieldSelectProjectionSyntax(
                FieldReferenceParser.Parse(trimmedExpression, allowWildcard: true),
                string.Empty);
        }

        var match = CoalescePattern().Match(trimmedExpression);
        if (!match.Success)
        {
            throw new FormatException($"SELECT expression '{expression}' is invalid.");
        }

        var fieldReference = FieldReferenceParser.Parse(match.Groups["field"].Value, allowWildcard: false);
        var defaultValue = UnescapeStringLiteral(match.Groups["default"].Value);
        return new CoalesceSelectProjectionSyntax(fieldReference, defaultValue, string.Empty);
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

    private const string CoalesceFunctionName = "COALESCE";
}
