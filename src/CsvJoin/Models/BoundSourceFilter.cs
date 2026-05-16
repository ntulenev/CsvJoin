using System.Globalization;

namespace CsvJoin.Models;

/// <summary>
/// Represents a WHERE filter bound to a concrete source header.
/// </summary>
/// <param name="SourceSide">The join side that owns the filtered source.</param>
/// <param name="SourceField">The concrete source header.</param>
/// <param name="Operator">The filter operator.</param>
/// <param name="Value">The comparison value, when required by the operator.</param>
/// <param name="Values">The comparison values, when required by the operator.</param>
internal sealed record BoundSourceFilter(
    JoinSourceSide SourceSide,
    string SourceField,
    SourceFilterOperator Operator,
    string? Value = null,
    IReadOnlyList<string>? Values = null)
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
            SourceFilterOperator.GreaterThan => Compare(value, Value) > 0,
            SourceFilterOperator.GreaterThanOrEqual => Compare(value, Value) >= 0,
            SourceFilterOperator.LessThan => Compare(value, Value) < 0,
            SourceFilterOperator.LessThanOrEqual => Compare(value, Value) <= 0,
            SourceFilterOperator.In => Values is not null && Values.Any(candidate => string.Equals(value, candidate, StringComparison.Ordinal)),
            SourceFilterOperator.Contains => value is not null && Value is not null && value.Contains(Value, StringComparison.Ordinal),
            SourceFilterOperator.IsNull => value is null,
            SourceFilterOperator.IsNotNull => value is not null,
            _ => throw new InvalidOperationException($"Unsupported source filter operator '{Operator}'."),
        };
    }

    private static int Compare(string? rowValue, string? filterValue)
    {
        if (rowValue is null || filterValue is null)
        {
            return rowValue is null && filterValue is null ? 0 : rowValue is null ? -1 : 1;
        }

        if (decimal.TryParse(rowValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var rowNumber) &&
            decimal.TryParse(filterValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var filterNumber))
        {
            return rowNumber.CompareTo(filterNumber);
        }

        if (DateTimeOffset.TryParse(rowValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var rowDate) &&
            DateTimeOffset.TryParse(filterValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var filterDate))
        {
            return rowDate.CompareTo(filterDate);
        }

        return string.Compare(rowValue, filterValue, StringComparison.Ordinal);
    }
}
