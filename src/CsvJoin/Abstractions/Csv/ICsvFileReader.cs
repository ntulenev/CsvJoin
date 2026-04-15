using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Abstractions.Csv;

/// <summary>
/// Reads CSV data sources into in-memory datasets.
/// </summary>
internal interface ICsvFileReader
{
    /// <summary>
    /// Reads a CSV source into a dataset.
    /// </summary>
    /// <param name="alias">The source alias used in the join query.</param>
    /// <param name="source">The CSV source settings.</param>
    /// <param name="cancellationToken">Signals that the operation should be canceled.</param>
    /// <returns>The parsed CSV dataset.</returns>
    Task<CsvDataSet> ReadAsync(string alias, CsvSourceOptions source, CancellationToken cancellationToken);
}
