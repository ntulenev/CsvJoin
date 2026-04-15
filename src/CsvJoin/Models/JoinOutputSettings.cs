namespace CsvJoin.Models;

internal sealed record JoinOutputSettings(
    string ResultsDirectory,
    string Delimiter,
    int ConsoleMaxRows,
    bool OpenResultAfterBuild);
