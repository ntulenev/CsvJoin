namespace CsvJoin.Abstractions.Presentation;

/// <summary>
/// Opens generated result files using the operating system shell.
/// </summary>
internal interface IResultFileLauncher
{
    /// <summary>
    /// Attempts to open a file.
    /// </summary>
    /// <param name="filePath">The file path to open.</param>
    /// <param name="errorMessage">An error message when the file could not be opened; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when the file was opened; otherwise <see langword="false"/>.</returns>
    bool TryOpen(string filePath, out string? errorMessage);
}
