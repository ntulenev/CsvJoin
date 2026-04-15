using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Abstractions.Csv;

internal interface ICsvFileReader
{
    Task<CsvDataSet> ReadAsync(string alias, CsvSourceOptions source, CancellationToken cancellationToken);
}
