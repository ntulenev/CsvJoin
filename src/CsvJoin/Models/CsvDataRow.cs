namespace CsvJoin.Models;

internal sealed record CsvDataRow(int Index, IReadOnlyDictionary<string, string?> Values)
{
    public string GetJoinKey(string header)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(header);
        return Values.TryGetValue(header, out var value) ? value ?? string.Empty : string.Empty;
    }

    public string? GetValueOrDefault(string header, string? defaultValue = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(header);
        return Values.TryGetValue(header, out var value) ? value ?? defaultValue : defaultValue;
    }
}
