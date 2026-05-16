namespace CsvJoin.Models;

/// <summary>
/// Represents a selected output column before it is bound to a concrete dataset header.
/// </summary>
/// <param name="SourceAlias">The source alias referenced by the query.</param>
/// <param name="SourceField">The configured source field or wildcard marker.</param>
/// <param name="OutputField">The requested output field name.</param>
/// <param name="IsWildcard">Indicates whether the column selects all fields from the source.</param>
/// <param name="DefaultValue">The fallback value used when the projected value is missing.</param>
internal sealed record SelectColumn(
    string SourceAlias,
    string SourceField,
    string OutputField,
    bool IsWildcard = false,
    string? DefaultValue = null)
{
    /// <summary>
    /// Binds the column to a concrete dataset.
    /// </summary>
    /// <param name="dataSet">The dataset that provides the source field.</param>
    /// <param name="sourceSide">The join side represented by the dataset.</param>
    /// <returns>The bound output columns.</returns>
    public IReadOnlyList<BoundSelectColumn> Bind(CsvDataSet dataSet, JoinSourceSide sourceSide) =>
        Bind(dataSet, sourceSide, ColumnTypeRegistry.Empty);

    /// <summary>
    /// Binds the column to a concrete dataset with configured column types.
    /// </summary>
    /// <param name="dataSet">The dataset that provides the source field.</param>
    /// <param name="sourceSide">The join side represented by the dataset.</param>
    /// <param name="columnTypes">The configured column data types.</param>
    /// <returns>The bound output columns.</returns>
    public IReadOnlyList<BoundSelectColumn> Bind(
        CsvDataSet dataSet,
        JoinSourceSide sourceSide,
        ColumnTypeRegistry columnTypes)
    {
        ArgumentNullException.ThrowIfNull(dataSet);
        ArgumentNullException.ThrowIfNull(columnTypes);

        if (IsWildcard)
        {
            return dataSet.Headers
                .Select(header => new BoundSelectColumn(
                    sourceSide,
                    header,
                    header,
                    DataType: columnTypes.Resolve(dataSet.Alias, header)))
                .ToArray();
        }

        var actualHeader = dataSet.ResolveHeader(SourceField);
        return [new BoundSelectColumn(sourceSide, actualHeader, OutputField, DefaultValue, columnTypes.Resolve(dataSet.Alias, actualHeader))];
    }
}
