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

    /// <summary>
    /// Gets the text encoding used to read the CSV file.
    /// </summary>
    [MinLength(1)]
    public string Encoding { get; init; } = "utf-8";

    /// <summary>
    /// Gets a value indicating whether field values should be trimmed while reading.
    /// </summary>
    public bool TrimFields { get; init; }

    /// <summary>
    /// Gets values that should be treated as null while reading.
    /// </summary>
    public IReadOnlyList<string> NullValues { get; init; } = [];

    /// <summary>
    /// Gets the quote character used by the CSV source.
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(1)]
    public string Quote { get; init; } = "\"";

    /// <summary>
    /// Gets a value indicating whether blank lines should be ignored.
    /// </summary>
    public bool IgnoreBlankLines { get; init; } = true;
}
