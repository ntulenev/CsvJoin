using FluentAssertions;

using Moq;

using CsvJoin.Abstractions.Presentation;
using CsvJoin.Application;
using CsvJoin.Configuration;
using CsvJoin.Csv;
using CsvJoin.Models;
using CsvJoin.Presentation.Files;

namespace CsvJoin.Tests.Application;

public class CsvJoinApplicationEndToEndTests
{
    [Fact(DisplayName = "RunAsync creates joined csv file from configured sources.")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncCreatesJoinedCsvFileFromConfiguredSources()
    {
        // Arrange
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var leftPath = Path.Combine(tempDirectory, "left.csv");
            var rightPath = Path.Combine(tempDirectory, "right.csv");
            var resultsDirectory = Path.Combine(tempDirectory, "results");

            await File.WriteAllTextAsync(leftPath, "Id,Name\r\n1,Alice\r\n2,Bob\r\n");
            await File.WriteAllTextAsync(rightPath, "Id,Status\r\n1,Active\r\n3,Inactive\r\n");

            var settings = new AppSettings
            {
                Sources = new Dictionary<string, CsvSourceOptions>(StringComparer.OrdinalIgnoreCase)
                {
                    ["left"] = new CsvSourceOptions { FilePath = leftPath, Delimiter = "," },
                    ["right"] = new CsvSourceOptions { FilePath = rightPath, Delimiter = "," },
                },
                Query = "SELECT left.Id, left.Name, right.Status FROM left LEFT JOIN right ON left.Id = right.Id",
                Output = new OutputOptions
                {
                    ConsoleMaxRows = 10,
                    Delimiter = ",",
                    OpenResultAfterBuild = false,
                    ResultsDirectory = resultsDirectory,
                },
            };
            var configuredJoinJob = new ConfiguredJoinJobBinder(new ConfiguredJoinJobSettingsBinder(new CsvJoinQueryParser())).Bind(settings).Job!;

            JoinOutputFile? capturedOutputFile = null;
            var consoleOutputRendererMock = new Mock<IConsoleOutputRenderer>(MockBehavior.Strict);
            consoleOutputRendererMock.Setup(x => x.RenderHeader(It.IsAny<ConfiguredJoinJob>()));
            consoleOutputRendererMock.Setup(x => x.RenderResult(It.IsAny<CsvJoinResult>(), settings.Output.ConsoleMaxRows));
            consoleOutputRendererMock
                .Setup(x => x.PrintFileSaved(It.IsAny<JoinOutputFile>()))
                .Callback<JoinOutputFile>(outputFile => capturedOutputFile = outputFile);

            var resultFileLauncherMock = new Mock<IResultFileLauncher>(MockBehavior.Strict);

            var sut = new CsvJoinApplication(
                configuredJoinJob,
                new CsvFileReader(),
                new CsvJoinProcessor(),
                consoleOutputRendererMock.Object,
                new CsvResultFileWriter(),
                resultFileLauncherMock.Object);

            // Act
            var resultCode = await sut.RunAsync(CancellationToken.None);

            // Assert
            resultCode.Should().Be(0);
            capturedOutputFile.Should().NotBeNull();

            var outputFile = capturedOutputFile!;
            outputFile.RowCount.Should().Be(2);
            File.Exists(outputFile.FilePath).Should().BeTrue();

            var lines = await File.ReadAllLinesAsync(outputFile.FilePath);
            lines.Should().Equal("Id,Name,Status", "1,Alice,Active", "2,Bob,");
        }
        finally
        {
            DeleteTemporaryDirectory(tempDirectory);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }

    private static void DeleteTemporaryDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }
}
