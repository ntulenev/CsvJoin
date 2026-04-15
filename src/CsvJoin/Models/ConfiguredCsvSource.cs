using CsvJoin.Configuration;

namespace CsvJoin.Models;

/// <summary>
/// Represents a CSV source that was resolved for a configured join job.
/// </summary>
/// <param name="Alias">The query alias of the source.</param>
/// <param name="Options">The source options.</param>
internal sealed record ConfiguredCsvSource(string Alias, CsvSourceOptions Options)
{
    /// <summary>
    /// Gets the source file path.
    /// </summary>
    public string FilePath => Options.FilePath;

    /// <summary>
    /// Gets the source delimiter.
    /// </summary>
    public string Delimiter => Options.Delimiter;
}
