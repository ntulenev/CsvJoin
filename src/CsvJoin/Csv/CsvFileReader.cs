using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration;

using CsvJoin.Abstractions.Csv;
using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Csv;

internal sealed class CsvFileReader : ICsvFileReader
{
    public async Task<CsvDataSet> ReadAsync(string alias, CsvSourceOptions source, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        ArgumentNullException.ThrowIfNull(source);

        using var stream = File.OpenRead(source.FilePath);
        using var textReader = new StreamReader(stream);
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = source.Delimiter,
            HasHeaderRecord = true,
            BadDataFound = null,
            MissingFieldFound = null,
        };

        using var csv = new CsvReader(textReader, configuration);

        if (!await csv.ReadAsync().ConfigureAwait(false))
        {
            throw new InvalidOperationException($"CSV source '{alias}' is empty: {source.FilePath}");
        }

        csv.ReadHeader();
        var headers = csv.HeaderRecord?.ToArray() ?? [];
        if (headers.Length == 0)
        {
            throw new InvalidOperationException($"CSV source '{alias}' does not contain a header row.");
        }

        ValidateDuplicateHeaders(alias, headers);

        var rows = new List<CsvDataRow>();
        var rowIndex = 0;

        while (await csv.ReadAsync().ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in headers)
            {
                values[header] = csv.GetField(header);
            }

            rows.Add(new CsvDataRow(rowIndex, values));
            rowIndex++;
        }

        return new CsvDataSet(alias, source.FilePath, headers, rows);
    }

    private static void ValidateDuplicateHeaders(string alias, IReadOnlyList<string> headers)
    {
        var duplicates = headers
            .GroupBy(static header => header, StringComparer.OrdinalIgnoreCase)
            .Where(static group => group.Count() > 1)
            .Select(static group => group.Key)
            .ToArray();

        if (duplicates.Length == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"CSV source '{alias}' contains duplicate headers: {string.Join(", ", duplicates)}");
    }
}
