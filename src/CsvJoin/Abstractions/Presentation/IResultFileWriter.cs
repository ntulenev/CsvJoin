using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Abstractions.Presentation;

internal interface IResultFileWriter
{
    Task<JoinOutputFile> WriteAsync(
        CsvJoinResult result,
        AppSettings settings,
        CancellationToken cancellationToken);
}
