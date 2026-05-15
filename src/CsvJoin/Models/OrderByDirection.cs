namespace CsvJoin.Models;

/// <summary>
/// Defines the direction used by an ORDER BY column.
/// </summary>
internal enum OrderByDirection
{
    /// <summary>
    /// Sorts values from low to high.
    /// </summary>
    Ascending,

    /// <summary>
    /// Sorts values from high to low.
    /// </summary>
    Descending,
}
