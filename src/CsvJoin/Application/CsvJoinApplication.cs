using Microsoft.Extensions.Options;

using CsvJoin.Abstractions.Application;
using CsvJoin.Abstractions.Csv;
using CsvJoin.Abstractions.Presentation;
using CsvJoin.Configuration;

namespace CsvJoin.Application;

internal sealed class CsvJoinApplication : ICsvJoinApplication
{
    private readonly ICsvJoinQueryParser _queryParser;
    private readonly ICsvFileReader _csvFileReader;
    private readonly ICsvJoinProcessor _csvJoinProcessor;
    private readonly IConsoleOutputRenderer _consoleOutputRenderer;
    private readonly IResultFileWriter _resultFileWriter;
    private readonly IResultFileLauncher _resultFileLauncher;
    private readonly AppSettings _settings;

    public CsvJoinApplication(
        ICsvJoinQueryParser queryParser,
        ICsvFileReader csvFileReader,
        ICsvJoinProcessor csvJoinProcessor,
        IConsoleOutputRenderer consoleOutputRenderer,
        IResultFileWriter resultFileWriter,
        IResultFileLauncher resultFileLauncher,
        IOptions<AppSettings> options)
    {
        ArgumentNullException.ThrowIfNull(queryParser);
        ArgumentNullException.ThrowIfNull(csvFileReader);
        ArgumentNullException.ThrowIfNull(csvJoinProcessor);
        ArgumentNullException.ThrowIfNull(consoleOutputRenderer);
        ArgumentNullException.ThrowIfNull(resultFileWriter);
        ArgumentNullException.ThrowIfNull(resultFileLauncher);
        ArgumentNullException.ThrowIfNull(options);

        _queryParser = queryParser;
        _csvFileReader = csvFileReader;
        _csvJoinProcessor = csvJoinProcessor;
        _consoleOutputRenderer = consoleOutputRenderer;
        _resultFileWriter = resultFileWriter;
        _resultFileLauncher = resultFileLauncher;
        _settings = options.Value;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var query = _queryParser.Parse(_settings.Query);
        _consoleOutputRenderer.RenderHeader(_settings, query);

        var leftSource = ResolveSource(query.LeftAlias);
        var rightSource = ResolveSource(query.RightAlias);

        var leftTask = _csvFileReader.ReadAsync(query.LeftAlias, leftSource, cancellationToken);
        var rightTask = _csvFileReader.ReadAsync(query.RightAlias, rightSource, cancellationToken);

        await Task.WhenAll(leftTask, rightTask).ConfigureAwait(false);

        var result = _csvJoinProcessor.Process(query, await leftTask.ConfigureAwait(false), await rightTask.ConfigureAwait(false));
        _consoleOutputRenderer.RenderResult(result, _settings.Output.ConsoleMaxRows);

        var outputFile = await _resultFileWriter.WriteAsync(result, _settings, cancellationToken).ConfigureAwait(false);
        _consoleOutputRenderer.PrintFileSaved(outputFile);

        if (!_settings.Output.OpenResultAfterBuild)
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

    private CsvSourceOptions ResolveSource(string alias)
    {
        if (_settings.Sources.TryGetValue(alias, out var source))
        {
            return source;
        }

        throw new InvalidOperationException($"CSV source '{alias}' is missing in configuration.");
    }
}
