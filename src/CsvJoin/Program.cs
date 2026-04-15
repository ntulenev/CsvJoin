using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Spectre.Console;

using CsvJoin.Abstractions.Application;
using CsvJoin.Abstractions.Csv;
using CsvJoin.Abstractions.Presentation;
using CsvJoin.Application;
using CsvJoin.Configuration;
using CsvJoin.Csv;
using CsvJoin.Presentation.Console;
using CsvJoin.Presentation.Files;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

builder.Services
    .AddOptions<AppSettings>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<AppSettings>, AppSettingsValidator>();
builder.Services.AddTransient<ICsvJoinQueryParser, CsvJoinQueryParser>();
builder.Services.AddTransient<ICsvFileReader, CsvFileReader>();
builder.Services.AddTransient<ICsvJoinProcessor, CsvJoinProcessor>();
builder.Services.AddTransient<IConsoleOutputRenderer, SpectreConsoleOutputRenderer>();
builder.Services.AddTransient<IResultFileWriter, CsvResultFileWriter>();
builder.Services.AddTransient<IResultFileLauncher, ShellResultFileLauncher>();
builder.Services.AddTransient<ICsvJoinApplication, CsvJoinApplication>();

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
