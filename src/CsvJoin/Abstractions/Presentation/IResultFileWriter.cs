using CsvJoin.Models;

namespace CsvJoin.Abstractions.Presentation;

/// <summary>
/// Writes join results to output files.
/// </summary>
internal interface IResultFileWriter
{
    /// <summary>
    /// Writes a join result to a file.
    /// </summary>
    /// <param name="result">The join result to write.</param>
    /// <param name="output">The output settings for the generated file.</param>
    /// <param name="cancellationToken">Signals that the operation should be canceled.</param>
    /// <returns>Metadata about the generated output file.</returns>
    Task<JoinOutputFile> WriteAsync(
        CsvJoinResult result,
        JoinOutputSettings output,
        CancellationToken cancellationToken);
}
