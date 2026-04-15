namespace CsvJoin.Models;

internal sealed record SelectColumn(
    string SourceAlias,
    string SourceField,
    string OutputField,
    bool IsWildcard = false,
    string? DefaultValue = null);
