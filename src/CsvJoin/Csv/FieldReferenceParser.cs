using System.Text.RegularExpressions;

using CsvJoin.Csv.Syntax;

namespace CsvJoin.Csv;

/// <summary>
/// Parses field references in the form <c>alias.Column</c> or <c>alias.[Column Name]</c>.
/// </summary>
internal static partial class FieldReferenceParser
{
    /// <summary>
    /// Parses a field reference expression.
    /// </summary>
    /// <param name="expression">The field reference text.</param>
    /// <param name="allowWildcard">Indicates whether <c>*</c> is allowed as field reference.</param>
    /// <returns>The parsed field reference syntax node.</returns>
    public static FieldReferenceSyntax Parse(string expression, bool allowWildcard)
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

            return new FieldReferenceSyntax(alias, fieldToken, true);
        }

        return new FieldReferenceSyntax(alias, UnwrapIdentifier(fieldToken), false);
    }

    /// <summary>
    /// Removes identifier escaping from a field or alias token.
    /// </summary>
    /// <param name="token">The identifier token.</param>
    /// <returns>The unwrapped identifier.</returns>
    public static string UnwrapIdentifier(string token)
    {
        var trimmed = token.Trim();
        if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
        {
            return trimmed[1..^1];
        }

        return trimmed;
    }

    [GeneratedRegex(
        @"^(?<alias>[A-Za-z_][A-Za-z0-9_]*)\.(?<field>\*|\[[^\]]+\]|[A-Za-z_][A-Za-z0-9_]*)$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex FieldPattern();
}
