namespace CsvJoin.Models;

internal sealed record BoundSelectColumn(
    JoinSourceSide SourceSide,
    string SourceField,
    string OutputField,
    string? DefaultValue = null)
{
    public string? Project(CsvDataRow? leftRow, CsvDataRow? rightRow)
    {
        var sourceRow = SourceSide == JoinSourceSide.Left ? leftRow : rightRow;
        return sourceRow?.GetValueOrDefault(SourceField, DefaultValue) ?? DefaultValue;
    }
}
