namespace CsvJoin.Abstractions.Presentation;

internal interface IResultFileLauncher
{
    bool TryOpen(string filePath, out string? errorMessage);
}
