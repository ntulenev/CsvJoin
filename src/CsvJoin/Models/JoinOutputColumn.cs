namespace CsvJoin.Models;

internal sealed record JoinOutputColumn(string SourceAlias, string SourceField, string OutputField);
