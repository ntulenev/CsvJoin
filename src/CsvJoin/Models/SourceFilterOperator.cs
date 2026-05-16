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
    /// Matches values greater than the configured value.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Matches values greater than or equal to the configured value.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Matches values less than the configured value.
    /// </summary>
    LessThan,

    /// <summary>
    /// Matches values less than or equal to the configured value.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Matches values contained in a configured set.
    /// </summary>
    In,

    /// <summary>
    /// Matches values containing the configured value.
    /// </summary>
    Contains,

    /// <summary>
    /// Matches null values.
    /// </summary>
    IsNull,

    /// <summary>
    /// Matches non-null values.
    /// </summary>
    IsNotNull,
}
