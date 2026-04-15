using System.ComponentModel.DataAnnotations;

namespace CsvJoin.Configuration;

/// <summary>
/// Defines how the join result should be written and displayed.
/// </summary>
internal sealed class OutputOptions
{
    /// <summary>
    /// Gets the directory where result files are written.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string ResultsDirectory { get; init; } = "results";

    /// <summary>
    /// Gets the delimiter used in the generated result file.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string Delimiter { get; init; } = ",";

    /// <summary>
    /// Gets the maximum number of rows shown in the console preview.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int ConsoleMaxRows { get; init; } = 50;

    /// <summary>
    /// Gets a value indicating whether the generated file should be opened after completion.
    /// </summary>
    public bool OpenResultAfterBuild { get; init; } = true;
}
