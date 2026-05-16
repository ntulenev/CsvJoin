namespace CsvJoin.Models;

/// <summary>
/// Resolves configured source column data types.
/// </summary>
internal sealed class ColumnTypeRegistry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnTypeRegistry"/> class.
    /// </summary>
    /// <param name="columnTypes">Column types keyed by alias and source field.</param>
    public ColumnTypeRegistry(IReadOnlyDictionary<string, ColumnDataType> columnTypes)
    {
        ArgumentNullException.ThrowIfNull(columnTypes);
        _columnTypes = new Dictionary<string, ColumnDataType>(columnTypes, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets an empty column type registry.
    /// </summary>
    public static ColumnTypeRegistry Empty { get; } = new(new Dictionary<string, ColumnDataType>(StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Resolves a source column data type.
    /// </summary>
    /// <param name="sourceAlias">The source alias.</param>
    /// <param name="sourceField">The concrete source field.</param>
    /// <returns>The configured data type, or text when none is configured.</returns>
    public ColumnDataType Resolve(string sourceAlias, string sourceField)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceAlias);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceField);

        return _columnTypes.TryGetValue(BuildKey(sourceAlias, sourceField), out var dataType)
            ? dataType
            : ColumnDataType.Text;
    }

    private static string BuildKey(string sourceAlias, string sourceField) => $"{sourceAlias}.{sourceField}";

    private readonly Dictionary<string, ColumnDataType> _columnTypes;
}
