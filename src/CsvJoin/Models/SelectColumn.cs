namespace CsvJoin.Models;

internal sealed record SelectColumn(
    string SourceAlias,
    string SourceField,
    string OutputField,
    bool IsWildcard = false,
    string? DefaultValue = null)
{
    public IReadOnlyList<BoundSelectColumn> Bind(CsvDataSet dataSet, JoinSourceSide sourceSide)
    {
        ArgumentNullException.ThrowIfNull(dataSet);

        if (IsWildcard)
        {
            return dataSet.Headers
                .Select(header => new BoundSelectColumn(sourceSide, header, header))
                .ToArray();
        }

        var actualHeader = dataSet.ResolveHeader(SourceField);
        return [new BoundSelectColumn(sourceSide, actualHeader, OutputField, DefaultValue)];
    }
}
