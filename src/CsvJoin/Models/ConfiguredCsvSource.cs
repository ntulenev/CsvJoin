using CsvJoin.Configuration;

namespace CsvJoin.Models;

internal sealed record ConfiguredCsvSource(string Alias, CsvSourceOptions Options)
{
    public string FilePath => Options.FilePath;

    public string Delimiter => Options.Delimiter;
}
