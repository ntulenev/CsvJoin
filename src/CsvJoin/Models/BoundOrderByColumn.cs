namespace CsvJoin.Models;

/// <summary>
/// Represents a resolved output column used to sort result rows.
/// </summary>
/// <param name="OutputField">The resolved output field name.</param>
/// <param name="Index">The output column index.</param>
/// <param name="Direction">The sort direction.</param>
/// <param name="DataType">The output column data type.</param>
internal sealed record BoundOrderByColumn(
    string OutputField,
    int Index,
    OrderByDirection Direction,
    ColumnDataType DataType = ColumnDataType.Text);
