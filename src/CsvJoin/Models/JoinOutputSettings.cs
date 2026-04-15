namespace CsvJoin.Models;

/// <summary>
/// Represents output settings used during result generation.
/// </summary>
/// <param name="ResultsDirectory">The directory where result files are written.</param>
/// <param name="Delimiter">The delimiter used in the output file.</param>
/// <param name="ConsoleMaxRows">The maximum number of rows shown in the console preview.</param>
/// <param name="OpenResultAfterBuild">Indicates whether the result file should be opened automatically.</param>
internal sealed record JoinOutputSettings(
    string ResultsDirectory,
    string Delimiter,
    int ConsoleMaxRows,
    bool OpenResultAfterBuild);
