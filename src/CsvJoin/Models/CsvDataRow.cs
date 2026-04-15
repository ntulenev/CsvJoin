namespace CsvJoin.Models;

internal sealed record CsvDataRow(int Index, IReadOnlyDictionary<string, string?> Values);
