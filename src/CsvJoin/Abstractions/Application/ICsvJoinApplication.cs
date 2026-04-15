namespace CsvJoin.Abstractions.Application;

internal interface ICsvJoinApplication
{
    Task<int> RunAsync(CancellationToken cancellationToken);
}
