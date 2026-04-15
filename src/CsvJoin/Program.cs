using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using CsvJoin;
using CsvJoin.Abstractions.Application;

var builder = Host.CreateApplicationBuilder(args);

CsvJoinCompositionRoot.ConfigureConfiguration(builder.Configuration);
CsvJoinCompositionRoot.AddCsvJoinServices(builder.Services, builder.Configuration);

using var host = builder.Build();
using var cancellationSource = new CancellationTokenSource();

ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationSource.Cancel();
};

Console.CancelKeyPress += cancelHandler;

try
{
    var application = host.Services.GetRequiredService<ICsvJoinApplication>();
    return await application.RunAsync(cancellationSource.Token).ConfigureAwait(false);
}
catch (OperationCanceledException) when (cancellationSource.IsCancellationRequested)
{
    return 130;
}
#pragma warning disable CA1031
catch (Exception exception)
{
    AnsiConsole.MarkupLine($"[bold red]Error:[/] {Markup.Escape(exception.Message)}");
    return 1;
}
#pragma warning restore CA1031
finally
{
    Console.CancelKeyPress -= cancelHandler;
}
