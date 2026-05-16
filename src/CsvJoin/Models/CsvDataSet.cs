namespace CsvJoin.Models;

/// <summary>
/// Represents an in-memory CSV dataset.
/// </summary>
internal sealed class CsvDataSet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvDataSet"/> class.
    /// </summary>
    /// <param name="alias">The source alias used in the query.</param>
    /// <param name="filePath">The source file path.</param>
    /// <param name="headers">The dataset headers.</param>
    /// <param name="rows">The dataset rows.</param>
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

    /// <summary>
    /// Gets the dataset alias.
    /// </summary>
    public string Alias { get; }

    /// <summary>
    /// Gets the dataset file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the dataset headers.
    /// </summary>
    public IReadOnlyList<string> Headers { get; }

    /// <summary>
    /// Gets the dataset rows.
    /// </summary>
    public IReadOnlyList<CsvDataRow> Rows { get; }

    /// <summary>
    /// Binds a select column to this dataset.
    /// </summary>
    /// <param name="selectColumn">The select column to bind.</param>
    /// <param name="sourceSide">The join side represented by this dataset.</param>
    /// <returns>The bound output columns.</returns>
    public IReadOnlyList<BoundSelectColumn> Bind(SelectColumn selectColumn, JoinSourceSide sourceSide)
    {
        ArgumentNullException.ThrowIfNull(selectColumn);
        return selectColumn.Bind(this, sourceSide);
    }

    /// <summary>
    /// Binds a select column to this dataset with configured column types.
    /// </summary>
    /// <param name="selectColumn">The select column to bind.</param>
    /// <param name="sourceSide">The join side represented by this dataset.</param>
    /// <param name="columnTypes">The configured column data types.</param>
    /// <returns>The bound output columns.</returns>
    public IReadOnlyList<BoundSelectColumn> Bind(
        SelectColumn selectColumn,
        JoinSourceSide sourceSide,
        ColumnTypeRegistry columnTypes)
    {
        ArgumentNullException.ThrowIfNull(selectColumn);
        return selectColumn.Bind(this, sourceSide, columnTypes);
    }

    /// <summary>
    /// Resolves a configured header name to its actual dataset header.
    /// </summary>
    /// <param name="configuredHeader">The configured header reference.</param>
    /// <returns>The actual dataset header.</returns>
    public string ResolveHeader(string configuredHeader)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuredHeader);

        if (_headerLookup.TryGetValue(configuredHeader, out var actualHeader))
        {
            return actualHeader;
        }

        throw new InvalidOperationException($"Column '{configuredHeader}' was not found in source '{Alias}'.");
    }

    /// <summary>
    /// Builds a lookup for rows grouped by the specified join header.
    /// </summary>
    /// <param name="joinHeader">The join header to use as key.</param>
    /// <returns>A lookup of join keys to matching dataset rows.</returns>
    public IReadOnlyDictionary<string, IReadOnlyList<CsvDataRow>> BuildLookup(string joinHeader) =>
        BuildLookup(joinHeader, new JoinKeyNormalizationSettings(false, false));

    /// <summary>
    /// Builds a lookup for rows grouped by the specified join header.
    /// </summary>
    /// <param name="joinHeader">The join header to use as key.</param>
    /// <param name="normalization">The join key normalization settings.</param>
    /// <returns>A lookup of join keys to matching dataset rows.</returns>
    public IReadOnlyDictionary<string, IReadOnlyList<CsvDataRow>> BuildLookup(
        string joinHeader,
        JoinKeyNormalizationSettings normalization)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(joinHeader);
        ArgumentNullException.ThrowIfNull(normalization);

        var resolvedHeader = ResolveHeader(joinHeader);
        var lookup = new Dictionary<string, List<CsvDataRow>>(normalization.Comparer);

        foreach (var row in Rows)
        {
            var joinKey = row.GetJoinKey(resolvedHeader, normalization);
            if (!lookup.TryGetValue(joinKey, out var bucket))
            {
                bucket = [];
                lookup[joinKey] = bucket;
            }

            bucket.Add(row);
        }

        return lookup.ToDictionary(static entry => entry.Key, static entry => (IReadOnlyList<CsvDataRow>)entry.Value, normalization.Comparer);
    }

    private readonly Dictionary<string, string> _headerLookup;
}
