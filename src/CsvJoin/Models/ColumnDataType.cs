namespace CsvJoin.Models;

/// <summary>
/// Defines the data type used for filtering and ordering column values.
/// </summary>
internal enum ColumnDataType
{
    /// <summary>
    /// Compares values as ordinal text.
    /// </summary>
    Text,

    /// <summary>
    /// Compares values as invariant-culture numbers.
    /// </summary>
    Number,

    /// <summary>
    /// Compares values as invariant-culture date/time values.
    /// </summary>
    Date,
}
