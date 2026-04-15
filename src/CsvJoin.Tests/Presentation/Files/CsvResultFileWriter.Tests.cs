using FluentAssertions;

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
        var output = new JoinOutputSettings(tempDirectory.Path, ",", 10, false);
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
        var outputFile = await sut.WriteAsync(result, output, CancellationToken.None);
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
        var output = new JoinOutputSettings("results", ",", 10, false);

        // Act
        Func<Task> action = () => sut.WriteAsync(null!, output, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "CsvResultFileWriter WriteAsync throws when output settings are null.")]
    [Trait("Category", "Unit")]
    public async Task WriteAsyncThrowsWhenOutputSettingsAreNull()
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

    [Fact(DisplayName = "CsvResultFileWriter WriteAsync uses configured delimiter.")]
    [Trait("Category", "Unit")]
    public async Task WriteAsyncUsesConfiguredDelimiter()
    {
        // Arrange
        using var tempDirectory = new TempDirectory();
        var sut = new CsvResultFileWriter();
        var output = new JoinOutputSettings(tempDirectory.Path, ";", 10, false);
        var headers = new[] { "Id", "Status" };
        var rows = new[] { (IReadOnlyList<string?>)new string?[] { "1", "Active" } };
        var result = new CsvJoinResult("left.csv", "right.csv", headers, rows);

        // Act
        var outputFile = await sut.WriteAsync(result, output, CancellationToken.None);
        var content = await File.ReadAllTextAsync(outputFile.FilePath);

        // Assert
        content.Should().Contain("Id;Status");
        content.Should().Contain("1;Active");
    }

    [Fact(DisplayName = "CsvResultFileWriter WriteAsync sanitizes invalid file name characters.")]
    [Trait("Category", "Unit")]
    public async Task WriteAsyncSanitizesInvalidFileNameCharacters()
    {
        // Arrange
        using var tempDirectory = new TempDirectory();
        var sut = new CsvResultFileWriter();
        var output = new JoinOutputSettings(tempDirectory.Path, ",", 10, false);
        var result = new CsvJoinResult("left:source?.csv", "right*source|.csv", ["Id"], []);

        // Act
        var outputFile = await sut.WriteAsync(result, output, CancellationToken.None);
        var fileName = Path.GetFileName(outputFile.FilePath);

        // Assert
        fileName.Should().NotContain(":");
        fileName.Should().NotContain("?");
        fileName.Should().NotContain("*");
        fileName.Should().NotContain("|");
    }

    [Fact(DisplayName = "CsvResultFileWriter WriteAsync honors cancellation.")]
    [Trait("Category", "Unit")]
    public async Task WriteAsyncHonorsCancellation()
    {
        // Arrange
        using var tempDirectory = new TempDirectory();
        using var cancellationSource = new CancellationTokenSource();
        var sut = new CsvResultFileWriter();
        var output = new JoinOutputSettings(tempDirectory.Path, ",", 10, false);
        var rows = Enumerable.Range(1, 5)
            .Select(index => (IReadOnlyList<string?>)new string?[] { index.ToString(System.Globalization.CultureInfo.InvariantCulture) })
            .ToArray();
        var result = new CsvJoinResult("left.csv", "right.csv", ["Id"], rows);
        await cancellationSource.CancelAsync();

        // Act
        Func<Task> action = () => sut.WriteAsync(result, output, cancellationSource.Token);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
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
