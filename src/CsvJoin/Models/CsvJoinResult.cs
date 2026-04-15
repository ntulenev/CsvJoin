namespace CsvJoin.Models;

/// <summary>
/// Represents the in-memory result of a join execution.
/// </summary>
/// <param name="LeftFilePath">The left source file path.</param>
/// <param name="RightFilePath">The right source file path.</param>
/// <param name="Headers">The output headers.</param>
/// <param name="Rows">The output rows.</param>
internal sealed record CsvJoinResult(
    string LeftFilePath,
    string RightFilePath,
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<string?>> Rows);
