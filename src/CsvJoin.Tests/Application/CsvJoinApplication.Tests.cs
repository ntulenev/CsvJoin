using FluentAssertions;

using Moq;

using Microsoft.Extensions.Options;

using CsvJoin.Abstractions.Application;
using CsvJoin.Abstractions.Csv;
using CsvJoin.Abstractions.Presentation;
using CsvJoin.Application;
using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Tests.Application;

public class CsvJoinApplicationTests
{
    [Fact(DisplayName = "CsvJoinApplication can be created.")]
    [Trait("Category", "Unit")]
    public void CsvJoinApplicationCanBeCreated()
    {
        // Arrange
        var configuredJoinJobFactory = new Mock<IConfiguredJoinJobFactory>(MockBehavior.Strict).Object;
        var csvFileReader = new Mock<ICsvFileReader>(MockBehavior.Strict).Object;
        var csvJoinProcessor = new Mock<ICsvJoinProcessor>(MockBehavior.Strict).Object;
        var consoleOutputRenderer = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict).Object;
        var resultFileWriter = new Mock<IResultFileWriter>(MockBehavior.Strict).Object;
        var resultFileLauncher = new Mock<IResultFileLauncher>(MockBehavior.Strict).Object;

        // Act
        var exception = Record.Exception(() => new CsvJoinApplication(
            configuredJoinJobFactory,
            csvFileReader,
            csvJoinProcessor,
            consoleOutputRenderer,
            resultFileWriter,
            resultFileLauncher,
            CreateOptions()));

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = "CsvJoinApplication constructor throws when configured join job factory is null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenConfiguredJoinJobFactoryIsNull()
    {
        // Arrange
        var csvFileReader = new Mock<ICsvFileReader>(MockBehavior.Strict).Object;
        var csvJoinProcessor = new Mock<ICsvJoinProcessor>(MockBehavior.Strict).Object;
        var consoleOutputRenderer = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict).Object;
        var resultFileWriter = new Mock<IResultFileWriter>(MockBehavior.Strict).Object;
        var resultFileLauncher = new Mock<IResultFileLauncher>(MockBehavior.Strict).Object;

        // Act
        Action action = () => _ = new CsvJoinApplication(null!, csvFileReader, csvJoinProcessor, consoleOutputRenderer, resultFileWriter, resultFileLauncher, CreateOptions());

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("configuredJoinJobFactory");
    }

    [Fact(DisplayName = "CsvJoinApplication constructor throws when csv file reader is null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenCsvFileReaderIsNull()
    {
        // Arrange
        var configuredJoinJobFactory = new Mock<IConfiguredJoinJobFactory>(MockBehavior.Strict).Object;
        var csvJoinProcessor = new Mock<ICsvJoinProcessor>(MockBehavior.Strict).Object;
        var consoleOutputRenderer = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict).Object;
        var resultFileWriter = new Mock<IResultFileWriter>(MockBehavior.Strict).Object;
        var resultFileLauncher = new Mock<IResultFileLauncher>(MockBehavior.Strict).Object;

        // Act
        Action action = () => _ = new CsvJoinApplication(configuredJoinJobFactory, null!, csvJoinProcessor, consoleOutputRenderer, resultFileWriter, resultFileLauncher, CreateOptions());

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("csvFileReader");
    }

    [Fact(DisplayName = "CsvJoinApplication constructor throws when csv join processor is null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenCsvJoinProcessorIsNull()
    {
        // Arrange
        var configuredJoinJobFactory = new Mock<IConfiguredJoinJobFactory>(MockBehavior.Strict).Object;
        var csvFileReader = new Mock<ICsvFileReader>(MockBehavior.Strict).Object;
        var consoleOutputRenderer = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict).Object;
        var resultFileWriter = new Mock<IResultFileWriter>(MockBehavior.Strict).Object;
        var resultFileLauncher = new Mock<IResultFileLauncher>(MockBehavior.Strict).Object;

        // Act
        Action action = () => _ = new CsvJoinApplication(configuredJoinJobFactory, csvFileReader, null!, consoleOutputRenderer, resultFileWriter, resultFileLauncher, CreateOptions());

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("csvJoinProcessor");
    }

    [Fact(DisplayName = "CsvJoinApplication constructor throws when console output renderer is null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenConsoleOutputRendererIsNull()
    {
        // Arrange
        var configuredJoinJobFactory = new Mock<IConfiguredJoinJobFactory>(MockBehavior.Strict).Object;
        var csvFileReader = new Mock<ICsvFileReader>(MockBehavior.Strict).Object;
        var csvJoinProcessor = new Mock<ICsvJoinProcessor>(MockBehavior.Strict).Object;
        var resultFileWriter = new Mock<IResultFileWriter>(MockBehavior.Strict).Object;
        var resultFileLauncher = new Mock<IResultFileLauncher>(MockBehavior.Strict).Object;

        // Act
        Action action = () => _ = new CsvJoinApplication(configuredJoinJobFactory, csvFileReader, csvJoinProcessor, null!, resultFileWriter, resultFileLauncher, CreateOptions());

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("consoleOutputRenderer");
    }

    [Fact(DisplayName = "CsvJoinApplication constructor throws when result file writer is null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenResultFileWriterIsNull()
    {
        // Arrange
        var configuredJoinJobFactory = new Mock<IConfiguredJoinJobFactory>(MockBehavior.Strict).Object;
        var csvFileReader = new Mock<ICsvFileReader>(MockBehavior.Strict).Object;
        var csvJoinProcessor = new Mock<ICsvJoinProcessor>(MockBehavior.Strict).Object;
        var consoleOutputRenderer = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict).Object;
        var resultFileLauncher = new Mock<IResultFileLauncher>(MockBehavior.Strict).Object;

        // Act
        Action action = () => _ = new CsvJoinApplication(configuredJoinJobFactory, csvFileReader, csvJoinProcessor, consoleOutputRenderer, null!, resultFileLauncher, CreateOptions());

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("resultFileWriter");
    }

    [Fact(DisplayName = "CsvJoinApplication constructor throws when result file launcher is null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenResultFileLauncherIsNull()
    {
        // Arrange
        var configuredJoinJobFactory = new Mock<IConfiguredJoinJobFactory>(MockBehavior.Strict).Object;
        var csvFileReader = new Mock<ICsvFileReader>(MockBehavior.Strict).Object;
        var csvJoinProcessor = new Mock<ICsvJoinProcessor>(MockBehavior.Strict).Object;
        var consoleOutputRenderer = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict).Object;
        var resultFileWriter = new Mock<IResultFileWriter>(MockBehavior.Strict).Object;

        // Act
        Action action = () => _ = new CsvJoinApplication(configuredJoinJobFactory, csvFileReader, csvJoinProcessor, consoleOutputRenderer, resultFileWriter, null!, CreateOptions());

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("resultFileLauncher");
    }

    [Fact(DisplayName = "CsvJoinApplication constructor throws when options are null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenOptionsAreNull()
    {
        // Arrange
        var configuredJoinJobFactory = new Mock<IConfiguredJoinJobFactory>(MockBehavior.Strict).Object;
        var csvFileReader = new Mock<ICsvFileReader>(MockBehavior.Strict).Object;
        var csvJoinProcessor = new Mock<ICsvJoinProcessor>(MockBehavior.Strict).Object;
        var consoleOutputRenderer = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict).Object;
        var resultFileWriter = new Mock<IResultFileWriter>(MockBehavior.Strict).Object;
        var resultFileLauncher = new Mock<IResultFileLauncher>(MockBehavior.Strict).Object;

        // Act
        Action action = () => _ = new CsvJoinApplication(configuredJoinJobFactory, csvFileReader, csvJoinProcessor, consoleOutputRenderer, resultFileWriter, resultFileLauncher, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact(DisplayName = "RunAsync renders result and opens file when configured.")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncRendersResultAndOpensFileWhenConfigured()
    {
        // Arrange
        var settings = CreateSettings(openResultAfterBuild: true);
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Left, CreateSelectColumns());
        var job = CreateJob(settings, query);
        var leftDataSet = CreateDataSet("left", job.LeftSource.FilePath);
        var rightDataSet = CreateDataSet("right", job.RightSource.FilePath);
        var resultHeaders = new[] { "Id", "Status" };
        var resultRows = new[] { (IReadOnlyList<string?>)new string?[] { "1", "Active" } };
        var result = new CsvJoinResult(leftDataSet.FilePath, rightDataSet.FilePath, resultHeaders, resultRows);
        var outputFile = new JoinOutputFile("joined.csv", 1);

        var configuredJoinJobFactoryMock = new Mock<IConfiguredJoinJobFactory>(MockBehavior.Strict);
        configuredJoinJobFactoryMock.Setup(x => x.Create(settings)).Returns(job);

        var csvFileReaderMock = new Mock<ICsvFileReader>(MockBehavior.Strict);
        csvFileReaderMock.Setup(x => x.ReadAsync(job.LeftSource.Alias, job.LeftSource.Options, It.IsAny<CancellationToken>())).ReturnsAsync(leftDataSet);
        csvFileReaderMock.Setup(x => x.ReadAsync(job.RightSource.Alias, job.RightSource.Options, It.IsAny<CancellationToken>())).ReturnsAsync(rightDataSet);

        var csvJoinProcessorMock = new Mock<ICsvJoinProcessor>(MockBehavior.Strict);
        csvJoinProcessorMock.Setup(x => x.Process(query, leftDataSet, rightDataSet)).Returns(result);

        var consoleOutputRendererMock = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict);
        consoleOutputRendererMock.Setup(x => x.RenderHeader(job));
        consoleOutputRendererMock.Setup(x => x.RenderResult(result, job.Output.ConsoleMaxRows));
        consoleOutputRendererMock.Setup(x => x.PrintFileSaved(outputFile));
        consoleOutputRendererMock.Setup(x => x.PrintFileOpened(outputFile.FilePath));

        var resultFileWriterMock = new Mock<IResultFileWriter>(MockBehavior.Strict);
        resultFileWriterMock.Setup(x => x.WriteAsync(result, job.Output, It.IsAny<CancellationToken>())).ReturnsAsync(outputFile);

        var resultFileLauncherMock = new Mock<IResultFileLauncher>(MockBehavior.Strict);
        resultFileLauncherMock
            .Setup(x => x.TryOpen(outputFile.FilePath, out It.Ref<string?>.IsAny))
            .Returns((string _, out string? errorMessage) =>
            {
                errorMessage = null;
                return true;
            });

        var sut = new CsvJoinApplication(
            configuredJoinJobFactoryMock.Object,
            csvFileReaderMock.Object,
            csvJoinProcessorMock.Object,
            consoleOutputRendererMock.Object,
            resultFileWriterMock.Object,
            resultFileLauncherMock.Object,
            Options.Create(settings));

        // Act
        var resultCode = await sut.RunAsync(CancellationToken.None);

        // Assert
        resultCode.Should().Be(0);
    }

    [Fact(DisplayName = "RunAsync prints warning when file open fails.")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncPrintsWarningWhenFileOpenFails()
    {
        // Arrange
        var settings = CreateSettings(openResultAfterBuild: true);
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, CreateSelectColumns());
        var job = CreateJob(settings, query);
        var leftDataSet = CreateDataSet("left", job.LeftSource.FilePath);
        var rightDataSet = CreateDataSet("right", job.RightSource.FilePath);
        var joinResult = new CsvJoinResult(leftDataSet.FilePath, rightDataSet.FilePath, ["Id"], []);
        var outputFile = new JoinOutputFile("joined.csv", 0);
        const string expectedError = "Open failed";

        var configuredJoinJobFactoryMock = new Mock<IConfiguredJoinJobFactory>(MockBehavior.Strict);
        configuredJoinJobFactoryMock.Setup(x => x.Create(settings)).Returns(job);

        var csvFileReaderMock = new Mock<ICsvFileReader>(MockBehavior.Strict);
        csvFileReaderMock.Setup(x => x.ReadAsync(job.LeftSource.Alias, job.LeftSource.Options, It.IsAny<CancellationToken>())).ReturnsAsync(leftDataSet);
        csvFileReaderMock.Setup(x => x.ReadAsync(job.RightSource.Alias, job.RightSource.Options, It.IsAny<CancellationToken>())).ReturnsAsync(rightDataSet);

        var csvJoinProcessorMock = new Mock<ICsvJoinProcessor>(MockBehavior.Strict);
        csvJoinProcessorMock.Setup(x => x.Process(query, leftDataSet, rightDataSet)).Returns(joinResult);

        var consoleOutputRendererMock = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict);
        consoleOutputRendererMock.Setup(x => x.RenderHeader(job));
        consoleOutputRendererMock.Setup(x => x.RenderResult(joinResult, job.Output.ConsoleMaxRows));
        consoleOutputRendererMock.Setup(x => x.PrintFileSaved(outputFile));
        consoleOutputRendererMock.Setup(x => x.PrintFileOpenWarning(expectedError));

        var resultFileWriterMock = new Mock<IResultFileWriter>(MockBehavior.Strict);
        resultFileWriterMock.Setup(x => x.WriteAsync(joinResult, job.Output, It.IsAny<CancellationToken>())).ReturnsAsync(outputFile);

        var resultFileLauncherMock = new Mock<IResultFileLauncher>(MockBehavior.Strict);
        resultFileLauncherMock
            .Setup(x => x.TryOpen(outputFile.FilePath, out It.Ref<string?>.IsAny))
            .Returns((string _, out string? errorMessage) =>
            {
                errorMessage = expectedError;
                return false;
            });

        var sut = new CsvJoinApplication(
            configuredJoinJobFactoryMock.Object,
            csvFileReaderMock.Object,
            csvJoinProcessorMock.Object,
            consoleOutputRendererMock.Object,
            resultFileWriterMock.Object,
            resultFileLauncherMock.Object,
            Options.Create(settings));

        // Act
        var resultCode = await sut.RunAsync(CancellationToken.None);

        // Assert
        resultCode.Should().Be(0);
    }

    [Fact(DisplayName = "RunAsync does not open file when disabled in settings.")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncDoesNotOpenFileWhenDisabledInSettings()
    {
        // Arrange
        var settings = CreateSettings(openResultAfterBuild: false);
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, CreateSelectColumns());
        var job = CreateJob(settings, query);
        var leftDataSet = CreateDataSet("left", job.LeftSource.FilePath);
        var rightDataSet = CreateDataSet("right", job.RightSource.FilePath);
        var joinResult = new CsvJoinResult(leftDataSet.FilePath, rightDataSet.FilePath, ["Id"], []);
        var outputFile = new JoinOutputFile("joined.csv", 0);

        var configuredJoinJobFactoryMock = new Mock<IConfiguredJoinJobFactory>(MockBehavior.Strict);
        configuredJoinJobFactoryMock.Setup(x => x.Create(settings)).Returns(job);

        var csvFileReaderMock = new Mock<ICsvFileReader>(MockBehavior.Strict);
        csvFileReaderMock.Setup(x => x.ReadAsync(job.LeftSource.Alias, job.LeftSource.Options, It.IsAny<CancellationToken>())).ReturnsAsync(leftDataSet);
        csvFileReaderMock.Setup(x => x.ReadAsync(job.RightSource.Alias, job.RightSource.Options, It.IsAny<CancellationToken>())).ReturnsAsync(rightDataSet);

        var csvJoinProcessorMock = new Mock<ICsvJoinProcessor>(MockBehavior.Strict);
        csvJoinProcessorMock.Setup(x => x.Process(query, leftDataSet, rightDataSet)).Returns(joinResult);

        var consoleOutputRendererMock = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict);
        consoleOutputRendererMock.Setup(x => x.RenderHeader(job));
        consoleOutputRendererMock.Setup(x => x.RenderResult(joinResult, job.Output.ConsoleMaxRows));
        consoleOutputRendererMock.Setup(x => x.PrintFileSaved(outputFile));

        var resultFileWriterMock = new Mock<IResultFileWriter>(MockBehavior.Strict);
        resultFileWriterMock.Setup(x => x.WriteAsync(joinResult, job.Output, It.IsAny<CancellationToken>())).ReturnsAsync(outputFile);

        var resultFileLauncherMock = new Mock<IResultFileLauncher>(MockBehavior.Strict);

        var sut = new CsvJoinApplication(
            configuredJoinJobFactoryMock.Object,
            csvFileReaderMock.Object,
            csvJoinProcessorMock.Object,
            consoleOutputRendererMock.Object,
            resultFileWriterMock.Object,
            resultFileLauncherMock.Object,
            Options.Create(settings));

        // Act
        var resultCode = await sut.RunAsync(CancellationToken.None);

        // Assert
        resultCode.Should().Be(0);
    }

    [Fact(DisplayName = "RunAsync does not print warning when launcher returns false with empty message.")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncDoesNotPrintWarningWhenLauncherReturnsFalseWithEmptyMessage()
    {
        // Arrange
        var settings = CreateSettings(openResultAfterBuild: true);
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, CreateSelectColumns());
        var job = CreateJob(settings, query);
        var leftDataSet = CreateDataSet("left", job.LeftSource.FilePath);
        var rightDataSet = CreateDataSet("right", job.RightSource.FilePath);
        var joinResult = new CsvJoinResult(leftDataSet.FilePath, rightDataSet.FilePath, ["Id"], []);
        var outputFile = new JoinOutputFile("joined.csv", 0);

        var configuredJoinJobFactoryMock = new Mock<IConfiguredJoinJobFactory>(MockBehavior.Strict);
        configuredJoinJobFactoryMock.Setup(x => x.Create(settings)).Returns(job);

        var csvFileReaderMock = new Mock<ICsvFileReader>(MockBehavior.Strict);
        csvFileReaderMock.Setup(x => x.ReadAsync(job.LeftSource.Alias, job.LeftSource.Options, It.IsAny<CancellationToken>())).ReturnsAsync(leftDataSet);
        csvFileReaderMock.Setup(x => x.ReadAsync(job.RightSource.Alias, job.RightSource.Options, It.IsAny<CancellationToken>())).ReturnsAsync(rightDataSet);

        var csvJoinProcessorMock = new Mock<ICsvJoinProcessor>(MockBehavior.Strict);
        csvJoinProcessorMock.Setup(x => x.Process(query, leftDataSet, rightDataSet)).Returns(joinResult);

        var consoleOutputRendererMock = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict);
        consoleOutputRendererMock.Setup(x => x.RenderHeader(job));
        consoleOutputRendererMock.Setup(x => x.RenderResult(joinResult, job.Output.ConsoleMaxRows));
        consoleOutputRendererMock.Setup(x => x.PrintFileSaved(outputFile));

        var resultFileWriterMock = new Mock<IResultFileWriter>(MockBehavior.Strict);
        resultFileWriterMock.Setup(x => x.WriteAsync(joinResult, job.Output, It.IsAny<CancellationToken>())).ReturnsAsync(outputFile);

        var resultFileLauncherMock = new Mock<IResultFileLauncher>(MockBehavior.Strict);
        resultFileLauncherMock
            .Setup(x => x.TryOpen(outputFile.FilePath, out It.Ref<string?>.IsAny))
            .Returns((string _, out string? errorMessage) =>
            {
                errorMessage = string.Empty;
                return false;
            });

        var sut = new CsvJoinApplication(
            configuredJoinJobFactoryMock.Object,
            csvFileReaderMock.Object,
            csvJoinProcessorMock.Object,
            consoleOutputRendererMock.Object,
            resultFileWriterMock.Object,
            resultFileLauncherMock.Object,
            Options.Create(settings));

        // Act
        var resultCode = await sut.RunAsync(CancellationToken.None);

        // Assert
        resultCode.Should().Be(0);
    }

    [Fact(DisplayName = "RunAsync throws when cancellation is requested before writing output file.")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncThrowsWhenCancellationIsRequestedBeforeWritingOutputFile()
    {
        // Arrange
        var settings = CreateSettings(openResultAfterBuild: false);
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, CreateSelectColumns());
        var job = CreateJob(settings, query);
        var leftDataSet = CreateDataSet("left", job.LeftSource.FilePath);
        var rightDataSet = CreateDataSet("right", job.RightSource.FilePath);
        var joinResult = new CsvJoinResult(leftDataSet.FilePath, rightDataSet.FilePath, ["Id"], []);
        using var cancellationSource = new CancellationTokenSource();

        var configuredJoinJobFactoryMock = new Mock<IConfiguredJoinJobFactory>(MockBehavior.Strict);
        configuredJoinJobFactoryMock.Setup(x => x.Create(settings)).Returns(job);

        var csvFileReaderMock = new Mock<ICsvFileReader>(MockBehavior.Strict);
        csvFileReaderMock.Setup(x => x.ReadAsync(job.LeftSource.Alias, job.LeftSource.Options, It.IsAny<CancellationToken>())).ReturnsAsync(leftDataSet);
        csvFileReaderMock.Setup(x => x.ReadAsync(job.RightSource.Alias, job.RightSource.Options, It.IsAny<CancellationToken>())).ReturnsAsync(rightDataSet);

        var csvJoinProcessorMock = new Mock<ICsvJoinProcessor>(MockBehavior.Strict);
        csvJoinProcessorMock
            .Setup(x => x.Process(query, leftDataSet, rightDataSet))
            .Callback(() => cancellationSource.Cancel())
            .Returns(joinResult);

        var consoleOutputRendererMock = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict);
        consoleOutputRendererMock.Setup(x => x.RenderHeader(job));
        consoleOutputRendererMock.Setup(x => x.RenderResult(joinResult, job.Output.ConsoleMaxRows));

        var resultFileWriterMock = new Mock<IResultFileWriter>(MockBehavior.Strict);
        resultFileWriterMock
            .Setup(x => x.WriteAsync(joinResult, job.Output, It.IsAny<CancellationToken>()))
            .Returns<CsvJoinResult, JoinOutputSettings, CancellationToken>((_, _, cancellationToken) =>
                Task.FromCanceled<JoinOutputFile>(cancellationToken));

        var resultFileLauncherMock = new Mock<IResultFileLauncher>(MockBehavior.Strict);

        var sut = new CsvJoinApplication(
            configuredJoinJobFactoryMock.Object,
            csvFileReaderMock.Object,
            csvJoinProcessorMock.Object,
            consoleOutputRendererMock.Object,
            resultFileWriterMock.Object,
            resultFileLauncherMock.Object,
            Options.Create(settings));

        // Act
        Func<Task> action = () => sut.RunAsync(cancellationSource.Token);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    private static IOptions<AppSettings> CreateOptions() => Options.Create(CreateSettings(openResultAfterBuild: false));

    private static AppSettings CreateSettings(bool openResultAfterBuild)
    {
        return new AppSettings
        {
            Sources = new Dictionary<string, CsvSourceOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["left"] = new CsvSourceOptions { FilePath = "left.csv" },
                ["right"] = new CsvSourceOptions { FilePath = "right.csv" },
            },
            Query = "SELECT left.Id, right.Status FROM left INNER JOIN right ON left.Id = right.Id",
            Output = new OutputOptions
            {
                ConsoleMaxRows = 10,
                OpenResultAfterBuild = openResultAfterBuild,
                ResultsDirectory = "results",
                Delimiter = ",",
            },
        };
    }

    private static ConfiguredJoinJob CreateJob(AppSettings settings, CsvJoinQuery query)
    {
        return new ConfiguredJoinJob(
            query,
            new ConfiguredCsvSource(query.LeftAlias, settings.Sources[query.LeftAlias]),
            new ConfiguredCsvSource(query.RightAlias, settings.Sources[query.RightAlias]),
            new JoinOutputSettings(
                settings.Output.ResultsDirectory,
                settings.Output.Delimiter,
                settings.Output.ConsoleMaxRows,
                settings.Output.OpenResultAfterBuild));
    }

    private static CsvDataSet CreateDataSet(string alias, string filePath)
    {
        var headers = new[] { "Id", "Status" };
        var rows = new[]
        {
            new CsvDataRow(0, new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["Id"] = "1",
                ["Status"] = "Active",
            }),
        };

        return new CsvDataSet(alias, filePath, headers, rows);
    }

    private static SelectColumn[] CreateSelectColumns()
    {
        var columns = new[]
        {
            new SelectColumn("left", "Id", "Id"),
            new SelectColumn("right", "Status", "Status"),
        };

        return columns;
    }
}
