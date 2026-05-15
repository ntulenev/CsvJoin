namespace CsvJoin.Models;

/// <summary>
/// Defines operators supported by source WHERE filters.
/// </summary>
internal enum SourceFilterOperator
{
    /// <summary>
    /// Matches values equal to the configured value.
    /// </summary>
    Equals,

    /// <summary>
    /// Matches values not equal to the configured value.
    /// </summary>
    NotEquals,

    /// <summary>
    /// Matches null values.
    /// </summary>
    IsNull,

    /// <summary>
    /// Matches non-null values.
    /// </summary>
    IsNotNull,
}
