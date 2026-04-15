using System.ComponentModel.DataAnnotations;

namespace CsvJoin.Configuration;

internal sealed class CsvSourceOptions
{
    [Required]
    [MinLength(1)]
    public required string FilePath { get; init; }

    [Required]
    [MinLength(1)]
    public string Delimiter { get; init; } = ",";
}
