using CsvJoin.Abstractions.Application;
using CsvJoin.Abstractions.Csv;
using CsvJoin.Abstractions.Presentation;
using CsvJoin.Models;

namespace CsvJoin.Application;

/// <summary>
/// Orchestrates the end-to-end CSV join workflow.
/// </summary>
internal sealed class CsvJoinApplication : ICsvJoinApplication
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvJoinApplication"/> class.
    /// </summary>
    /// <param name="configuredJoinJob">The configured join job to execute.</param>
    /// <param name="csvFileReader">The CSV reader.</param>
    /// <param name="csvJoinProcessor">The join processor.</param>
    /// <param name="consoleOutputRenderer">The console renderer.</param>
    /// <param name="resultFileWriter">The result file writer.</param>
    /// <param name="resultFileLauncher">The shell file launcher.</param>
    public CsvJoinApplication(
        ConfiguredJoinJob configuredJoinJob,
        ICsvFileReader csvFileReader,
        ICsvJoinProcessor csvJoinProcessor,
        IConsoleOutputRenderer consoleOutputRenderer,
        IResultFileWriter resultFileWriter,
        IResultFileLauncher resultFileLauncher)
    {
        ArgumentNullException.ThrowIfNull(configuredJoinJob);
        ArgumentNullException.ThrowIfNull(csvFileReader);
        ArgumentNullException.ThrowIfNull(csvJoinProcessor);
        ArgumentNullException.ThrowIfNull(consoleOutputRenderer);
        ArgumentNullException.ThrowIfNull(resultFileWriter);
        ArgumentNullException.ThrowIfNull(resultFileLauncher);

        _configuredJoinJob = configuredJoinJob;
        _csvFileReader = csvFileReader;
        _csvJoinProcessor = csvJoinProcessor;
        _consoleOutputRenderer = consoleOutputRenderer;
        _resultFileWriter = resultFileWriter;
        _resultFileLauncher = resultFileLauncher;
    }

    /// <inheritdoc />
    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        _consoleOutputRenderer.RenderHeader(_configuredJoinJob);

        var leftTask = _csvFileReader.ReadAsync(_configuredJoinJob.LeftSource.Alias, _configuredJoinJob.LeftSource.Options, cancellationToken);
        var rightTask = _csvFileReader.ReadAsync(_configuredJoinJob.RightSource.Alias, _configuredJoinJob.RightSource.Options, cancellationToken);

        await Task.WhenAll(leftTask, rightTask).ConfigureAwait(false);

        var result = _csvJoinProcessor.Process(_configuredJoinJob.Query, await leftTask.ConfigureAwait(false), await rightTask.ConfigureAwait(false));
        _consoleOutputRenderer.RenderResult(result, _configuredJoinJob.Output.ConsoleMaxRows);

        var outputFile = await _resultFileWriter.WriteAsync(result, _configuredJoinJob.Output, cancellationToken).ConfigureAwait(false);
        _consoleOutputRenderer.PrintFileSaved(outputFile);

        if (!_configuredJoinJob.Output.OpenResultAfterBuild)
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

    private readonly ConfiguredJoinJob _configuredJoinJob;
    private readonly ICsvFileReader _csvFileReader;
    private readonly ICsvJoinProcessor _csvJoinProcessor;
    private readonly IConsoleOutputRenderer _consoleOutputRenderer;
    private readonly IResultFileWriter _resultFileWriter;
    private readonly IResultFileLauncher _resultFileLauncher;
}
