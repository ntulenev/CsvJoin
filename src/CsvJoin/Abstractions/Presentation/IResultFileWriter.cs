using CsvJoin.Models;

namespace CsvJoin.Abstractions.Presentation;

internal interface IResultFileWriter
{
    Task<JoinOutputFile> WriteAsync(
        CsvJoinResult result,
        JoinOutputSettings output,
        CancellationToken cancellationToken);
}
