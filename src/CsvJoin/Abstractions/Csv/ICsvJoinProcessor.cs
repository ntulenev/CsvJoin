using CsvJoin.Models;

namespace CsvJoin.Abstractions.Csv;

/// <summary>
/// Executes a bound join query against two datasets.
/// </summary>
internal interface ICsvJoinProcessor
{
    /// <summary>
    /// Processes a join query against the provided datasets.
    /// </summary>
    /// <param name="query">The parsed join query.</param>
    /// <param name="left">The left dataset.</param>
    /// <param name="right">The right dataset.</param>
    /// <returns>The join result.</returns>
    CsvJoinResult Process(CsvJoinQuery query, CsvDataSet left, CsvDataSet right);

    /// <summary>
    /// Processes a join query against the provided datasets using configured join key normalization.
    /// </summary>
    /// <param name="query">The parsed join query.</param>
    /// <param name="left">The left dataset.</param>
    /// <param name="right">The right dataset.</param>
    /// <param name="joinKeys">The join key normalization settings.</param>
    /// <returns>The join result.</returns>
    CsvJoinResult Process(
        CsvJoinQuery query,
        CsvDataSet left,
        CsvDataSet right,
        JoinKeyNormalizationSettings joinKeys);
}
