using System.Globalization;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

using CsvJoin.Abstractions.Csv;
using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Csv;

/// <summary>
/// Reads CSV files into <see cref="CsvDataSet"/> instances.
/// </summary>
internal sealed class CsvFileReader : ICsvFileReader
{
    /// <inheritdoc />
    public async Task<CsvDataSet> ReadAsync(string alias, CsvSourceOptions source, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        ArgumentNullException.ThrowIfNull(source);

        using var stream = File.OpenRead(source.FilePath);
        using var textReader = new StreamReader(stream, ResolveEncoding(source.Encoding), detectEncodingFromByteOrderMarks: true);
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = source.Delimiter,
            HasHeaderRecord = true,
            BadDataFound = null,
            IgnoreBlankLines = source.IgnoreBlankLines,
            MissingFieldFound = null,
            Quote = source.Quote[0],
            TrimOptions = source.TrimFields ? TrimOptions.Trim : TrimOptions.None,
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
                values[header] = NormalizeField(csv.GetField(header), source.NullValues);
            }

            rows.Add(new CsvDataRow(rowIndex, values));
            rowIndex++;
        }

        return new CsvDataSet(alias, source.FilePath, headers, rows);
    }

    private static Encoding ResolveEncoding(string encodingName)
    {
        try
        {
            return Encoding.GetEncoding(encodingName);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidOperationException($"CSV source encoding '{encodingName}' is not supported.", exception);
        }
    }

    private static string? NormalizeField(string? value, IReadOnlyList<string> nullValues)
    {
        if (value is null)
        {
            return null;
        }

        return nullValues.Contains(value, StringComparer.Ordinal) ? null : value;
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
