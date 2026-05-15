namespace CsvJoin.Models;

/// <summary>
/// Represents a WHERE filter applied to a source row before joining.
/// </summary>
/// <param name="SourceAlias">The source alias referenced by the filter.</param>
/// <param name="SourceField">The source field referenced by the filter.</param>
/// <param name="Operator">The filter operator.</param>
/// <param name="Value">The comparison value, when required by the operator.</param>
internal sealed record SourceFilter(
    string SourceAlias,
    string SourceField,
    SourceFilterOperator Operator,
    string? Value = null);
