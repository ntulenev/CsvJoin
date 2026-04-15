using System.ComponentModel.DataAnnotations;

namespace CsvJoin.Configuration;

/// <summary>
/// Represents the root application settings for a CSV join run.
/// </summary>
internal sealed class AppSettings
{
    /// <summary>
    /// Gets the configured CSV sources keyed by query alias.
    /// </summary>
    [Required]
    [MinLength(1)]
    public Dictionary<string, CsvSourceOptions> Sources { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the SQL-like join query text.
    /// </summary>
    [Required]
    [MinLength(1)]
    public required string Query { get; init; }

    /// <summary>
    /// Gets the output settings for the generated result.
    /// </summary>
    [Required]
    public OutputOptions Output { get; init; } = new();
}
