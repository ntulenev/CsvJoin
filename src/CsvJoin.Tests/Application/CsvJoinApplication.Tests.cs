using FluentAssertions;

using Moq;

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
        var configuredJoinJob = CreateJob(openResultAfterBuild: false);
        var csvFileReader = new Mock<ICsvFileReader>(MockBehavior.Strict).Object;
        var csvJoinProcessor = new Mock<ICsvJoinProcessor>(MockBehavior.Strict).Object;
        var consoleOutputRenderer = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict).Object;
        var resultFileWriter = new Mock<IResultFileWriter>(MockBehavior.Strict).Object;
        var resultFileLauncher = new Mock<IResultFileLauncher>(MockBehavior.Strict).Object;

        // Act
        var exception = Record.Exception(() => new CsvJoinApplication(
            configuredJoinJob,
            csvFileReader,
            csvJoinProcessor,
            consoleOutputRenderer,
            resultFileWriter,
            resultFileLauncher));

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = "CsvJoinApplication constructor throws when configured join job is null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenConfiguredJoinJobIsNull()
    {
        // Arrange
        var csvFileReader = new Mock<ICsvFileReader>(MockBehavior.Strict).Object;
        var csvJoinProcessor = new Mock<ICsvJoinProcessor>(MockBehavior.Strict).Object;
        var consoleOutputRenderer = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict).Object;
        var resultFileWriter = new Mock<IResultFileWriter>(MockBehavior.Strict).Object;
        var resultFileLauncher = new Mock<IResultFileLauncher>(MockBehavior.Strict).Object;

        // Act
        Action action = () => _ = new CsvJoinApplication(null!, csvFileReader, csvJoinProcessor, consoleOutputRenderer, resultFileWriter, resultFileLauncher);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("configuredJoinJob");
    }

    [Fact(DisplayName = "CsvJoinApplication constructor throws when csv file reader is null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenCsvFileReaderIsNull()
    {
        // Arrange
        var configuredJoinJob = CreateJob(openResultAfterBuild: false);
        var csvJoinProcessor = new Mock<ICsvJoinProcessor>(MockBehavior.Strict).Object;
        var consoleOutputRenderer = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict).Object;
        var resultFileWriter = new Mock<IResultFileWriter>(MockBehavior.Strict).Object;
        var resultFileLauncher = new Mock<IResultFileLauncher>(MockBehavior.Strict).Object;

        // Act
        Action action = () => _ = new CsvJoinApplication(configuredJoinJob, null!, csvJoinProcessor, consoleOutputRenderer, resultFileWriter, resultFileLauncher);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("csvFileReader");
    }

    [Fact(DisplayName = "CsvJoinApplication constructor throws when csv join processor is null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenCsvJoinProcessorIsNull()
    {
        // Arrange
        var configuredJoinJob = CreateJob(openResultAfterBuild: false);
        var csvFileReader = new Mock<ICsvFileReader>(MockBehavior.Strict).Object;
        var consoleOutputRenderer = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict).Object;
        var resultFileWriter = new Mock<IResultFileWriter>(MockBehavior.Strict).Object;
        var resultFileLauncher = new Mock<IResultFileLauncher>(MockBehavior.Strict).Object;

        // Act
        Action action = () => _ = new CsvJoinApplication(configuredJoinJob, csvFileReader, null!, consoleOutputRenderer, resultFileWriter, resultFileLauncher);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("csvJoinProcessor");
    }

    [Fact(DisplayName = "CsvJoinApplication constructor throws when console output renderer is null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenConsoleOutputRendererIsNull()
    {
        // Arrange
        var configuredJoinJob = CreateJob(openResultAfterBuild: false);
        var csvFileReader = new Mock<ICsvFileReader>(MockBehavior.Strict).Object;
        var csvJoinProcessor = new Mock<ICsvJoinProcessor>(MockBehavior.Strict).Object;
        var resultFileWriter = new Mock<IResultFileWriter>(MockBehavior.Strict).Object;
        var resultFileLauncher = new Mock<IResultFileLauncher>(MockBehavior.Strict).Object;

        // Act
        Action action = () => _ = new CsvJoinApplication(configuredJoinJob, csvFileReader, csvJoinProcessor, null!, resultFileWriter, resultFileLauncher);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("consoleOutputRenderer");
    }

    [Fact(DisplayName = "CsvJoinApplication constructor throws when result file writer is null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenResultFileWriterIsNull()
    {
        // Arrange
        var configuredJoinJob = CreateJob(openResultAfterBuild: false);
        var csvFileReader = new Mock<ICsvFileReader>(MockBehavior.Strict).Object;
        var csvJoinProcessor = new Mock<ICsvJoinProcessor>(MockBehavior.Strict).Object;
        var consoleOutputRenderer = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict).Object;
        var resultFileLauncher = new Mock<IResultFileLauncher>(MockBehavior.Strict).Object;

        // Act
        Action action = () => _ = new CsvJoinApplication(configuredJoinJob, csvFileReader, csvJoinProcessor, consoleOutputRenderer, null!, resultFileLauncher);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("resultFileWriter");
    }

    [Fact(DisplayName = "CsvJoinApplication constructor throws when result file launcher is null.")]
    [Trait("Category", "Unit")]
    public void ConstructorThrowsWhenResultFileLauncherIsNull()
    {
        // Arrange
        var configuredJoinJob = CreateJob(openResultAfterBuild: false);
        var csvFileReader = new Mock<ICsvFileReader>(MockBehavior.Strict).Object;
        var csvJoinProcessor = new Mock<ICsvJoinProcessor>(MockBehavior.Strict).Object;
        var consoleOutputRenderer = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict).Object;
        var resultFileWriter = new Mock<IResultFileWriter>(MockBehavior.Strict).Object;

        // Act
        Action action = () => _ = new CsvJoinApplication(configuredJoinJob, csvFileReader, csvJoinProcessor, consoleOutputRenderer, resultFileWriter, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("resultFileLauncher");
    }

    [Fact(DisplayName = "RunAsync renders result and opens file when configured.")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncRendersResultAndOpensFileWhenConfigured()
    {
        // Arrange
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Left, CreateSelectColumns());
        var job = CreateJob(query, openResultAfterBuild: true);
        var leftDataSet = CreateDataSet("left", job.LeftSource.FilePath);
        var rightDataSet = CreateDataSet("right", job.RightSource.FilePath);
        var resultHeaders = new[] { "Id", "Status" };
        var resultRows = new[] { (IReadOnlyList<string?>)new string?[] { "1", "Active" } };
        var result = new CsvJoinResult(leftDataSet.FilePath, rightDataSet.FilePath, resultHeaders, resultRows);
        var outputFile = new JoinOutputFile("joined.csv", 1);

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
            job,
            csvFileReaderMock.Object,
            csvJoinProcessorMock.Object,
            consoleOutputRendererMock.Object,
            resultFileWriterMock.Object,
            resultFileLauncherMock.Object);

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
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, CreateSelectColumns());
        var job = CreateJob(query, openResultAfterBuild: true);
        var leftDataSet = CreateDataSet("left", job.LeftSource.FilePath);
        var rightDataSet = CreateDataSet("right", job.RightSource.FilePath);
        var joinResult = new CsvJoinResult(leftDataSet.FilePath, rightDataSet.FilePath, ["Id"], []);
        var outputFile = new JoinOutputFile("joined.csv", 0);
        const string expectedError = "Open failed";

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
            job,
            csvFileReaderMock.Object,
            csvJoinProcessorMock.Object,
            consoleOutputRendererMock.Object,
            resultFileWriterMock.Object,
            resultFileLauncherMock.Object);

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
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, CreateSelectColumns());
        var job = CreateJob(query, openResultAfterBuild: false);
        var leftDataSet = CreateDataSet("left", job.LeftSource.FilePath);
        var rightDataSet = CreateDataSet("right", job.RightSource.FilePath);
        var joinResult = new CsvJoinResult(leftDataSet.FilePath, rightDataSet.FilePath, ["Id"], []);
        var outputFile = new JoinOutputFile("joined.csv", 0);

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
            job,
            csvFileReaderMock.Object,
            csvJoinProcessorMock.Object,
            consoleOutputRendererMock.Object,
            resultFileWriterMock.Object,
            resultFileLauncherMock.Object);

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
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, CreateSelectColumns());
        var job = CreateJob(query, openResultAfterBuild: true);
        var leftDataSet = CreateDataSet("left", job.LeftSource.FilePath);
        var rightDataSet = CreateDataSet("right", job.RightSource.FilePath);
        var joinResult = new CsvJoinResult(leftDataSet.FilePath, rightDataSet.FilePath, ["Id"], []);
        var outputFile = new JoinOutputFile("joined.csv", 0);

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
            job,
            csvFileReaderMock.Object,
            csvJoinProcessorMock.Object,
            consoleOutputRendererMock.Object,
            resultFileWriterMock.Object,
            resultFileLauncherMock.Object);

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
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, CreateSelectColumns());
        var job = CreateJob(query, openResultAfterBuild: false);
        var leftDataSet = CreateDataSet("left", job.LeftSource.FilePath);
        var rightDataSet = CreateDataSet("right", job.RightSource.FilePath);
        var joinResult = new CsvJoinResult(leftDataSet.FilePath, rightDataSet.FilePath, ["Id"], []);
        using var cancellationSource = new CancellationTokenSource();

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
            job,
            csvFileReaderMock.Object,
            csvJoinProcessorMock.Object,
            consoleOutputRendererMock.Object,
            resultFileWriterMock.Object,
            resultFileLauncherMock.Object);

        // Act
        Func<Task> action = () => sut.RunAsync(cancellationSource.Token);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    private static ConfiguredJoinJob CreateJob(bool openResultAfterBuild)
    {
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, CreateSelectColumns());
        return CreateJob(query, openResultAfterBuild);
    }

    private static ConfiguredJoinJob CreateJob(CsvJoinQuery query, bool openResultAfterBuild)
    {
        return new ConfiguredJoinJob(
            query,
            new ConfiguredCsvSource(query.LeftAlias, new CsvSourceOptions { FilePath = "left.csv", Delimiter = "," }),
            new ConfiguredCsvSource(query.RightAlias, new CsvSourceOptions { FilePath = "right.csv", Delimiter = "," }),
            new JoinOutputSettings("results", ",", 10, openResultAfterBuild));
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
