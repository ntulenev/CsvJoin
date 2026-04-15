using Microsoft.Extensions.Options;

using CsvJoin.Abstractions.Application;
using CsvJoin.Abstractions.Csv;
using CsvJoin.Abstractions.Presentation;
using CsvJoin.Configuration;

namespace CsvJoin.Application;

internal sealed class CsvJoinApplication : ICsvJoinApplication
{
    private readonly IConfiguredJoinJobFactory _configuredJoinJobFactory;
    private readonly ICsvFileReader _csvFileReader;
    private readonly ICsvJoinProcessor _csvJoinProcessor;
    private readonly IConsoleOutputRenderer _consoleOutputRenderer;
    private readonly IResultFileWriter _resultFileWriter;
    private readonly IResultFileLauncher _resultFileLauncher;
    private readonly AppSettings _settings;

    public CsvJoinApplication(
        IConfiguredJoinJobFactory configuredJoinJobFactory,
        ICsvFileReader csvFileReader,
        ICsvJoinProcessor csvJoinProcessor,
        IConsoleOutputRenderer consoleOutputRenderer,
        IResultFileWriter resultFileWriter,
        IResultFileLauncher resultFileLauncher,
        IOptions<AppSettings> options)
    {
        ArgumentNullException.ThrowIfNull(configuredJoinJobFactory);
        ArgumentNullException.ThrowIfNull(csvFileReader);
        ArgumentNullException.ThrowIfNull(csvJoinProcessor);
        ArgumentNullException.ThrowIfNull(consoleOutputRenderer);
        ArgumentNullException.ThrowIfNull(resultFileWriter);
        ArgumentNullException.ThrowIfNull(resultFileLauncher);
        ArgumentNullException.ThrowIfNull(options);

        _configuredJoinJobFactory = configuredJoinJobFactory;
        _csvFileReader = csvFileReader;
        _csvJoinProcessor = csvJoinProcessor;
        _consoleOutputRenderer = consoleOutputRenderer;
        _resultFileWriter = resultFileWriter;
        _resultFileLauncher = resultFileLauncher;
        _settings = options.Value;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var job = _configuredJoinJobFactory.Create(_settings);
        _consoleOutputRenderer.RenderHeader(job);

        var leftTask = _csvFileReader.ReadAsync(job.LeftSource.Alias, job.LeftSource.Options, cancellationToken);
        var rightTask = _csvFileReader.ReadAsync(job.RightSource.Alias, job.RightSource.Options, cancellationToken);

        await Task.WhenAll(leftTask, rightTask).ConfigureAwait(false);

        var result = _csvJoinProcessor.Process(job.Query, await leftTask.ConfigureAwait(false), await rightTask.ConfigureAwait(false));
        _consoleOutputRenderer.RenderResult(result, job.Output.ConsoleMaxRows);

        var outputFile = await _resultFileWriter.WriteAsync(result, job.Output, cancellationToken).ConfigureAwait(false);
        _consoleOutputRenderer.PrintFileSaved(outputFile);

        if (!job.Output.OpenResultAfterBuild)
        {
            return 0;
        }

        if (_resultFileLauncher.TryOpen(outputFile.FilePath, out var errorMessage))
        {
            _consoleOutputRenderer.PrintFileOpened(outputFile.FilePath);
        }
        else if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            _consoleOutputRenderer.PrintFileOpenWarning(errorMessage);
        }

        return 0;
    }
}
