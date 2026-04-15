namespace CsvJoin.Csv.Syntax;

/// <summary>
/// Represents a field reference in the DSL syntax tree.
/// </summary>
/// <param name="SourceAlias">The source alias referenced by the field.</param>
/// <param name="SourceField">The referenced source field.</param>
/// <param name="IsWildcard">Indicates whether the field reference is a wildcard.</param>
internal sealed record FieldReferenceSyntax(
    string SourceAlias,
    string SourceField,
    bool IsWildcard);
