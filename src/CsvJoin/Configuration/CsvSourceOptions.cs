using System.ComponentModel.DataAnnotations;

namespace CsvJoin.Configuration;

/// <summary>
/// Defines how a single CSV source should be read.
/// </summary>
internal sealed class CsvSourceOptions
{
    /// <summary>
    /// Gets the CSV file path.
    /// </summary>
    [Required]
    [MinLength(1)]
    public required string FilePath { get; init; }

    /// <summary>
    /// Gets the delimiter used in the source file.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string Delimiter { get; init; } = ",";
}
