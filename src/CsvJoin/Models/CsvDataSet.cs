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

    public IReadOnlyList<BoundSelectColumn> Bind(SelectColumn selectColumn, JoinSourceSide sourceSide)
    {
        ArgumentNullException.ThrowIfNull(selectColumn);
        return selectColumn.Bind(this, sourceSide);
    }

    public string ResolveHeader(string configuredHeader)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuredHeader);

        if (_headerLookup.TryGetValue(configuredHeader, out var actualHeader))
        {
            return actualHeader;
        }

        throw new InvalidOperationException($"Column '{configuredHeader}' was not found in source '{Alias}'.");
    }

    public IReadOnlyDictionary<string, IReadOnlyList<CsvDataRow>> BuildLookup(string joinHeader)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(joinHeader);

        var resolvedHeader = ResolveHeader(joinHeader);
        var lookup = new Dictionary<string, List<CsvDataRow>>(StringComparer.Ordinal);

        foreach (var row in Rows)
        {
            var joinKey = row.GetJoinKey(resolvedHeader);
            if (!lookup.TryGetValue(joinKey, out var bucket))
            {
                bucket = [];
                lookup[joinKey] = bucket;
            }

            bucket.Add(row);
        }

        return lookup.ToDictionary(static entry => entry.Key, static entry => (IReadOnlyList<CsvDataRow>)entry.Value, StringComparer.Ordinal);
    }
}
