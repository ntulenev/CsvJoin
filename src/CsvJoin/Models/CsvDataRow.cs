namespace CsvJoin.Models;

/// <summary>
/// Represents a single CSV data row.
/// </summary>
/// <param name="Index">The zero-based row index inside the dataset.</param>
/// <param name="Values">The row values keyed by header name.</param>
internal sealed record CsvDataRow(int Index, IReadOnlyDictionary<string, string?> Values)
{
    /// <summary>
    /// Gets the normalized join key for the specified header.
    /// </summary>
    /// <param name="header">The header that provides the join value.</param>
    /// <returns>The join key, or an empty string when no value exists.</returns>
    public string GetJoinKey(string header)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(header);
        return Values.TryGetValue(header, out var value) ? value ?? string.Empty : string.Empty;
    }

    /// <summary>
    /// Gets a row value or the provided default value when none exists.
    /// </summary>
    /// <param name="header">The header whose value should be returned.</param>
    /// <param name="defaultValue">The fallback value to use when the row value is missing.</param>
    /// <returns>The row value or the fallback value.</returns>
    public string? GetValueOrDefault(string header, string? defaultValue = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(header);
        return Values.TryGetValue(header, out var value) ? value ?? defaultValue : defaultValue;
    }
}
