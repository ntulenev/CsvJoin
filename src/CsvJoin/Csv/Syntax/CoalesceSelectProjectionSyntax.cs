using CsvJoin.Models;

namespace CsvJoin.Csv.Syntax;

/// <summary>
/// Represents a <c>COALESCE</c> projection in the DSL syntax tree.
/// </summary>
/// <param name="FieldReference">The projected field reference.</param>
/// <param name="DefaultValue">The fallback value used by the projection.</param>
/// <param name="OutputField">The requested output field name.</param>
internal sealed record CoalesceSelectProjectionSyntax(
    FieldReferenceSyntax FieldReference,
    string DefaultValue,
    string OutputField)
    : SelectProjectionSyntax(FieldReference, OutputField)
{
    /// <inheritdoc />
    public override SelectColumn ToSelectColumn() =>
        new(FieldReference.SourceAlias, FieldReference.SourceField, OutputField, FieldReference.IsWildcard, DefaultValue);
}
