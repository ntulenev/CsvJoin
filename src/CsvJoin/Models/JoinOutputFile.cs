namespace CsvJoin.Models;

/// <summary>
/// Represents a generated join output file.
/// </summary>
/// <param name="FilePath">The generated file path.</param>
/// <param name="RowCount">The number of rows written to the file.</param>
internal sealed record JoinOutputFile(string FilePath, int RowCount);
