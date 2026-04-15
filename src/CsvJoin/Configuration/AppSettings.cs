using System.ComponentModel.DataAnnotations;

namespace CsvJoin.Configuration;

internal sealed class AppSettings
{
    [Required]
    [MinLength(1)]
    public Dictionary<string, CsvSourceOptions> Sources { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    [Required]
    [MinLength(1)]
    public required string Query { get; init; }

    [Required]
    public OutputOptions Output { get; init; } = new();
}
