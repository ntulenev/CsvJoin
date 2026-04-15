namespace CsvJoin.Models;

/// <summary>
/// Represents a projected column after it has been bound to a concrete dataset header.
/// </summary>
/// <param name="SourceSide">The side of the join that provides the value.</param>
/// <param name="SourceField">The concrete source header name.</param>
/// <param name="OutputField">The output column name.</param>
/// <param name="DefaultValue">The fallback value used when the source value is missing.</param>
internal sealed record BoundSelectColumn(
    JoinSourceSide SourceSide,
    string SourceField,
    string OutputField,
    string? DefaultValue = null)
{
    /// <summary>
    /// Projects a value from the provided join rows.
    /// </summary>
    /// <param name="leftRow">The left join row, when available.</param>
    /// <param name="rightRow">The right join row, when available.</param>
    /// <returns>The projected cell value.</returns>
    public string? Project(CsvDataRow? leftRow, CsvDataRow? rightRow)
    {
        var sourceRow = SourceSide == JoinSourceSide.Left ? leftRow : rightRow;
        return sourceRow?.GetValueOrDefault(SourceField, DefaultValue) ?? DefaultValue;
    }
}
