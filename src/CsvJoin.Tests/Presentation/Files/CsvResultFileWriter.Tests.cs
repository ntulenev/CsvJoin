using FluentAssertions;

using CsvJoin.Configuration;
using CsvJoin.Models;
using CsvJoin.Presentation.Files;

namespace CsvJoin.Tests.Presentation.Files;

public class CsvResultFileWriterTests
{
    [Fact(DisplayName = "CsvResultFileWriter WriteAsync writes result file.")]
    [Trait("Category", "Unit")]
    public async Task WriteAsyncWritesResultFile()
    {
        // Arrange
        using var tempDirectory = new TempDirectory();
        var sut = new CsvResultFileWriter();
        var settings = new AppSettings
        {
            Query = "SELECT left.Id FROM left INNER JOIN right ON left.Id = right.Id",
            Output = new OutputOptions
            {
                ResultsDirectory = tempDirectory.Path,
                Delimiter = ",",
                OpenResultAfterBuild = false,
            },
        };
        var headers = new[] { "Id", "Status" };
        var rows = new[]
        {
            (IReadOnlyList<string?>)new string?[] { "1", "Active" },
            new string?[] { "2", null },
        };
        var result = new CsvJoinResult(
            "left.csv",
            "right.csv",
            headers,
            rows);

        // Act
        var outputFile = await sut.WriteAsync(result, settings, CancellationToken.None);
        var content = await File.ReadAllTextAsync(outputFile.FilePath);

        // Assert
        outputFile.RowCount.Should().Be(2);
        File.Exists(outputFile.FilePath).Should().BeTrue();
        content.Should().Contain("Id,Status");
        content.Should().Contain("1,Active");
    }

    [Fact(DisplayName = "CsvResultFileWriter WriteAsync throws when result is null.")]
    [Trait("Category", "Unit")]
    public async Task WriteAsyncThrowsWhenResultIsNull()
    {
        // Arrange
        var sut = new CsvResultFileWriter();
        var settings = new AppSettings { Query = "query" };

        // Act
        Func<Task> action = () => sut.WriteAsync(null!, settings, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "CsvResultFileWriter WriteAsync throws when settings are null.")]
    [Trait("Category", "Unit")]
    public async Task WriteAsyncThrowsWhenSettingsAreNull()
    {
        // Arrange
        var sut = new CsvResultFileWriter();
        var headers = new[] { "Id" };
        var result = new CsvJoinResult("left.csv", "right.csv", headers, []);

        // Act
        Func<Task> action = () => sut.WriteAsync(result, null!, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
