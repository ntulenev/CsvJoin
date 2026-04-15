using CsvJoin.Models;

namespace CsvJoin.Csv.Syntax;

/// <summary>
/// Represents a single projection in the DSL syntax tree.
/// </summary>
/// <param name="FieldReference">The projected field reference.</param>
/// <param name="OutputField">The requested output field name.</param>
internal abstract record SelectProjectionSyntax(
    FieldReferenceSyntax FieldReference,
    string OutputField)
{
    /// <summary>
    /// Converts the syntax node to a domain select column.
    /// </summary>
    /// <returns>The corresponding domain select column.</returns>
    public abstract SelectColumn ToSelectColumn();
}
