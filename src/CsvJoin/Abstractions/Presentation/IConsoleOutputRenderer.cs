using CsvJoin.Models;

namespace CsvJoin.Abstractions.Presentation;

/// <summary>
/// Renders execution progress and results to the console.
/// </summary>
internal interface IConsoleOutputRenderer
{
    /// <summary>
    /// Renders the execution header for a configured join job.
    /// </summary>
    /// <param name="job">The configured join job being executed.</param>
    void RenderHeader(ConfiguredJoinJob job);

    /// <summary>
    /// Renders a preview of the join result.
    /// </summary>
    /// <param name="result">The join result to render.</param>
    /// <param name="consoleMaxRows">The maximum number of rows to show in the console.</param>
    void RenderResult(CsvJoinResult result, int consoleMaxRows);

    /// <summary>
    /// Prints a message that the result file was saved.
    /// </summary>
    /// <param name="outputFile">The saved output file information.</param>
    void PrintFileSaved(JoinOutputFile outputFile);

    /// <summary>
    /// Prints a message that the result file was opened.
    /// </summary>
    /// <param name="filePath">The file path that was opened.</param>
    void PrintFileOpened(string filePath);

    /// <summary>
    /// Prints a warning that the result file could not be opened automatically.
    /// </summary>
    /// <param name="message">The warning message to display.</param>
    void PrintFileOpenWarning(string message);
}
