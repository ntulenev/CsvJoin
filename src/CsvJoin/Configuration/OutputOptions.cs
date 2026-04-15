using System.ComponentModel.DataAnnotations;

namespace CsvJoin.Configuration;

internal sealed class OutputOptions
{
    [Required]
    [MinLength(1)]
    public string ResultsDirectory { get; init; } = "results";

    [Required]
    [MinLength(1)]
    public string Delimiter { get; init; } = ",";

    [Range(0, int.MaxValue)]
    public int ConsoleMaxRows { get; init; } = 50;

    public bool OpenResultAfterBuild { get; init; } = true;
}
