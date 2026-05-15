namespace CsvJoin.Configuration;

/// <summary>
/// Defines how join keys should be normalized before matching.
/// </summary>
internal sealed class JoinKeyOptions
{
    /// <summary>
    /// Gets a value indicating whether leading and trailing whitespace should be removed from join keys.
    /// </summary>
    public bool TrimWhitespace { get; init; }

    /// <summary>
    /// Gets a value indicating whether join key matching should ignore character casing.
    /// </summary>
    public bool IgnoreCase { get; init; }
}
