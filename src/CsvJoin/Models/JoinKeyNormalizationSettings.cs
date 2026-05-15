namespace CsvJoin.Models;

/// <summary>
/// Represents normalization rules used when comparing join keys.
/// </summary>
/// <param name="TrimWhitespace">Indicates whether leading and trailing whitespace should be removed.</param>
/// <param name="IgnoreCase">Indicates whether key matching should ignore character casing.</param>
internal sealed record JoinKeyNormalizationSettings(bool TrimWhitespace, bool IgnoreCase)
{
    /// <summary>
    /// Gets the comparer used for normalized join keys.
    /// </summary>
    public StringComparer Comparer => IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    /// <summary>
    /// Normalizes a join key value.
    /// </summary>
    /// <param name="value">The source key value.</param>
    /// <returns>The normalized key value.</returns>
    public string Normalize(string? value)
    {
        var normalized = value ?? string.Empty;
        return TrimWhitespace ? normalized.Trim() : normalized;
    }
}
