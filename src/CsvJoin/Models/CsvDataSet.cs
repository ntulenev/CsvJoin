namespace CsvJoin.Models;

internal sealed class CsvDataSet
{
    private readonly Dictionary<string, string> _headerLookup;

    public CsvDataSet(
        string alias,
        string filePath,
        IReadOnlyList<string> headers,
        IReadOnlyList<CsvDataRow> rows)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(rows);

        Alias = alias;
        FilePath = filePath;
        Headers = headers;
        Rows = rows;
        _headerLookup = headers.ToDictionary(static header => header, static header => header, StringComparer.OrdinalIgnoreCase);
    }

    public string Alias { get; }

    public string FilePath { get; }

    public IReadOnlyList<string> Headers { get; }

    public IReadOnlyList<CsvDataRow> Rows { get; }

    public string ResolveHeader(string configuredHeader)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuredHeader);

        if (_headerLookup.TryGetValue(configuredHeader, out var actualHeader))
        {
            return actualHeader;
        }

        throw new InvalidOperationException($"Column '{configuredHeader}' was not found in source '{Alias}'.");
    }
}
