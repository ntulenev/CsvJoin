namespace CsvJoin.Models;

internal sealed record CsvJoinResult(
    string LeftFilePath,
    string RightFilePath,
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<string?>> Rows);
