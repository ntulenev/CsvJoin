using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration;

using CsvJoin.Abstractions.Presentation;
using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Presentation.Files;

internal sealed class CsvResultFileWriter : IResultFileWriter
{
    public async Task<JoinOutputFile> WriteAsync(
        CsvJoinResult result,
        AppSettings settings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(settings);

        var resultsDirectory = Path.GetFullPath(settings.Output.ResultsDirectory);
        Directory.CreateDirectory(resultsDirectory);

        var leftName = Path.GetFileNameWithoutExtension(result.LeftFilePath);
        var rightName = Path.GetFileNameWithoutExtension(result.RightFilePath);
        var fileName = $"{SanitizeFileSegment(leftName)}_{SanitizeFileSegment(rightName)}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = Path.Combine(resultsDirectory, fileName);

        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = settings.Output.Delimiter,
            HasHeaderRecord = true,
        };

        using var stream = File.Create(filePath);
        using var writer = new StreamWriter(stream);
        using var csv = new CsvWriter(writer, configuration);

        foreach (var header in result.Headers)
        {
            csv.WriteField(header);
        }

        await csv.NextRecordAsync().ConfigureAwait(false);

        foreach (var row in result.Rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var value in row)
            {
                csv.WriteField(value);
            }

            await csv.NextRecordAsync().ConfigureAwait(false);
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        return new JoinOutputFile(filePath, result.Rows.Count);
    }

    private static string SanitizeFileSegment(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(character => invalidChars.Contains(character) ? '_' : character).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "source" : sanitized;
    }
}
