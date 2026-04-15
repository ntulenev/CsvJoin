using CsvJoin.Models;

namespace CsvJoin.Csv.Syntax;

/// <summary>
/// Represents a simple field projection in the DSL syntax tree.
/// </summary>
/// <param name="FieldReference">The projected field reference.</param>
/// <param name="OutputField">The requested output field name.</param>
internal sealed record FieldSelectProjectionSyntax(
    FieldReferenceSyntax FieldReference,
    string OutputField)
    : SelectProjectionSyntax(FieldReference, OutputField)
{
    /// <inheritdoc />
    public override SelectColumn ToSelectColumn() =>
        new(FieldReference.SourceAlias, FieldReference.SourceField, OutputField, FieldReference.IsWildcard);
}
