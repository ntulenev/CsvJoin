namespace CsvJoin.Models;

internal sealed record CsvJoinQuery(
    string LeftAlias,
    string LeftJoinField,
    string RightAlias,
    string RightJoinField,
    JoinType JoinType,
    IReadOnlyList<SelectColumn> SelectColumns);
