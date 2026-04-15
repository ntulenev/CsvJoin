namespace CsvJoin.Csv.Syntax;

/// <summary>
/// Represents a join condition in the DSL syntax tree.
/// </summary>
/// <param name="LeftReference">The first field reference from the join condition.</param>
/// <param name="RightReference">The second field reference from the join condition.</param>
internal sealed record JoinConditionSyntax(
    FieldReferenceSyntax LeftReference,
    FieldReferenceSyntax RightReference)
{
    /// <summary>
    /// Resolves the join fields for the declared source aliases.
    /// </summary>
    /// <param name="leftAlias">The declared left alias.</param>
    /// <param name="rightAlias">The declared right alias.</param>
    /// <returns>The resolved left and right join field names.</returns>
    public (string LeftJoinField, string RightJoinField) ResolveJoinFields(string leftAlias, string rightAlias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(leftAlias);
        ArgumentException.ThrowIfNullOrWhiteSpace(rightAlias);

        if (string.Equals(LeftReference.SourceAlias, RightReference.SourceAlias, StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException("JOIN ON clause must reference both source aliases.");
        }

        var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { leftAlias, rightAlias };
        if (!aliases.Contains(LeftReference.SourceAlias) || !aliases.Contains(RightReference.SourceAlias))
        {
            throw new FormatException("JOIN ON clause references an alias that is not declared in FROM/JOIN.");
        }

        var leftJoinField = string.Equals(LeftReference.SourceAlias, leftAlias, StringComparison.OrdinalIgnoreCase)
            ? LeftReference.SourceField
            : RightReference.SourceField;
        var rightJoinField = string.Equals(LeftReference.SourceAlias, rightAlias, StringComparison.OrdinalIgnoreCase)
            ? LeftReference.SourceField
            : RightReference.SourceField;

        return (leftJoinField, rightJoinField);
    }
}
