namespace CsvJoin.Models;

/// <summary>
/// Identifies the left or right side of a join.
/// </summary>
internal enum JoinSourceSide
{
    /// <summary>
    /// The left side of the join.
    /// </summary>
    Left = 0,

    /// <summary>
    /// The right side of the join.
    /// </summary>
    Right = 1,
}
