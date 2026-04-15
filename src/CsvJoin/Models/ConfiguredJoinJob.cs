namespace CsvJoin.Models;

internal sealed record ConfiguredJoinJob(
    CsvJoinQuery Query,
    ConfiguredCsvSource LeftSource,
    ConfiguredCsvSource RightSource,
    JoinOutputSettings Output)
{
    public ConfiguredCsvSource ResolveSource(JoinSourceSide sourceSide) =>
        sourceSide == JoinSourceSide.Left ? LeftSource : RightSource;
}
