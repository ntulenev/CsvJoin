namespace CsvJoin.Models;

/// <summary>
/// Represents a fully configured join job ready for execution.
/// </summary>
/// <param name="Query">The parsed join query.</param>
/// <param name="LeftSource">The configured left source.</param>
/// <param name="RightSource">The configured right source.</param>
/// <param name="Output">The configured output settings.</param>
internal sealed record ConfiguredJoinJob(
    CsvJoinQuery Query,
    ConfiguredCsvSource LeftSource,
    ConfiguredCsvSource RightSource,
    JoinOutputSettings Output)
{
    /// <summary>
    /// Resolves the configured source for a join side.
    /// </summary>
    /// <param name="sourceSide">The side whose source should be returned.</param>
    /// <returns>The configured source for the requested side.</returns>
    public ConfiguredCsvSource ResolveSource(JoinSourceSide sourceSide) =>
        sourceSide == JoinSourceSide.Left ? LeftSource : RightSource;
}
