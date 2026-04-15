namespace CsvJoin.Models;

/// <summary>
/// Specifies the supported join types.
/// </summary>
internal enum JoinType
{
    /// <summary>
    /// Returns only matching rows from both sides.
    /// </summary>
    Inner,

    /// <summary>
    /// Returns all rows from the left side and matching rows from the right side.
    /// </summary>
    Left,

    /// <summary>
    /// Returns all rows from the right side and matching rows from the left side.
    /// </summary>
    Right,

    /// <summary>
    /// Returns all rows from both sides.
    /// </summary>
    Full,
}
