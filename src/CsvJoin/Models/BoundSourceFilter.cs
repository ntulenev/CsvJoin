namespace CsvJoin.Models;

/// <summary>
/// Represents a WHERE filter bound to a concrete source header.
/// </summary>
/// <param name="SourceSide">The join side that owns the filtered source.</param>
/// <param name="SourceField">The concrete source header.</param>
/// <param name="Operator">The filter operator.</param>
/// <param name="Value">The comparison value, when required by the operator.</param>
internal sealed record BoundSourceFilter(
    JoinSourceSide SourceSide,
    string SourceField,
    SourceFilterOperator Operator,
    string? Value = null)
{
    /// <summary>
    /// Returns whether the row satisfies the filter.
    /// </summary>
    /// <param name="row">The row to inspect.</param>
    /// <returns><see langword="true"/> when the row satisfies the filter.</returns>
    public bool Matches(CsvDataRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        var value = row.GetValueOrDefault(SourceField);
        return Operator switch
        {
            SourceFilterOperator.Equals => string.Equals(value, Value, StringComparison.Ordinal),
            SourceFilterOperator.NotEquals => !string.Equals(value, Value, StringComparison.Ordinal),
            SourceFilterOperator.IsNull => value is null,
            SourceFilterOperator.IsNotNull => value is not null,
            _ => throw new InvalidOperationException($"Unsupported source filter operator '{Operator}'."),
        };
    }
}
