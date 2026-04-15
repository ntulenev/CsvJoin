using FluentAssertions;

using CsvJoin.Configuration;
using CsvJoin.Csv;

namespace CsvJoin.Tests.Csv;

public class CsvFileReaderTests
{
    [Fact(DisplayName = "CsvFileReader ReadAsync reads headers and rows.")]
    [Trait("Category", "Unit")]
    public async Task ReadAsyncReadsHeadersAndRows()
    {
        // Arrange
        using var csvFile = TempCsvFile.Create("Id,Name\n1,Alice\n2,Bob\n");
        var sut = new CsvFileReader();
        var source = new CsvSourceOptions { FilePath = csvFile.Path, Delimiter = "," };

        // Act
        var result = await sut.ReadAsync("left", source, CancellationToken.None);

        // Assert
        result.Alias.Should().Be("left");
        result.Headers.Should().Equal("Id", "Name");
        result.Rows.Should().HaveCount(2);
        result.Rows[0].Values["Name"].Should().Be("Alice");
        result.Rows[1].Values["Id"].Should().Be("2");
    }

    [Fact(DisplayName = "CsvFileReader ReadAsync throws when alias is empty.")]
    [Trait("Category", "Unit")]
    public async Task ReadAsyncThrowsWhenAliasIsEmpty()
    {
        // Arrange
        var sut = new CsvFileReader();
        using var csvFile = TempCsvFile.Create("Id\n1\n");
        var source = new CsvSourceOptions { FilePath = csvFile.Path };

        // Act
        Func<Task> action = () => sut.ReadAsync(" ", source, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact(DisplayName = "CsvFileReader ReadAsync throws when source is null.")]
    [Trait("Category", "Unit")]
    public async Task ReadAsyncThrowsWhenSourceIsNull()
    {
        // Arrange
        var sut = new CsvFileReader();

        // Act
        Func<Task> action = () => sut.ReadAsync("left", null!, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "CsvFileReader ReadAsync throws for empty file.")]
    [Trait("Category", "Unit")]
    public async Task ReadAsyncThrowsForEmptyFile()
    {
        // Arrange
        using var csvFile = TempCsvFile.Create(string.Empty);
        var sut = new CsvFileReader();
        var source = new CsvSourceOptions { FilePath = csvFile.Path };

        // Act
        Func<Task> action = () => sut.ReadAsync("left", source, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*empty*");
    }

    [Fact(DisplayName = "CsvFileReader ReadAsync throws for duplicate headers.")]
    [Trait("Category", "Unit")]
    public async Task ReadAsyncThrowsForDuplicateHeaders()
    {
        // Arrange
        using var csvFile = TempCsvFile.Create("Id,ID\n1,2\n");
        var sut = new CsvFileReader();
        var source = new CsvSourceOptions { FilePath = csvFile.Path };

        // Act
        Func<Task> action = () => sut.ReadAsync("left", source, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*duplicate headers*");
    }

    [Fact(DisplayName = "CsvFileReader ReadAsync honors cancellation.")]
    [Trait("Category", "Unit")]
    public async Task ReadAsyncHonorsCancellation()
    {
        // Arrange
        using var csvFile = TempCsvFile.Create("Id,Name\n1,Alice\n");
        var sut = new CsvFileReader();
        var source = new CsvSourceOptions { FilePath = csvFile.Path };
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();

        // Act
        Func<Task> action = () => sut.ReadAsync("left", source, cancellationSource.Token);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact(DisplayName = "CsvFileReader ReadAsync supports custom delimiter.")]
    [Trait("Category", "Unit")]
    public async Task ReadAsyncSupportsCustomDelimiter()
    {
        // Arrange
        using var csvFile = TempCsvFile.Create("Id;Name\n1;Alice\n");
        var sut = new CsvFileReader();
        var source = new CsvSourceOptions { FilePath = csvFile.Path, Delimiter = ";" };

        // Act
        var result = await sut.ReadAsync("left", source, CancellationToken.None);

        // Assert
        result.Headers.Should().Equal("Id", "Name");
        result.Rows[0].Values["Name"].Should().Be("Alice");
    }

    [Fact(DisplayName = "CsvFileReader ReadAsync reads quoted fields.")]
    [Trait("Category", "Unit")]
    public async Task ReadAsyncReadsQuotedFields()
    {
        // Arrange
        using var csvFile = TempCsvFile.Create("Id,Name\n1,\"Alice, Bob\"\n");
        var sut = new CsvFileReader();
        var source = new CsvSourceOptions { FilePath = csvFile.Path, Delimiter = "," };

        // Act
        var result = await sut.ReadAsync("left", source, CancellationToken.None);

        // Assert
        result.Rows[0].Values["Name"].Should().Be("Alice, Bob");
    }

    private sealed class TempCsvFile(string path) : IDisposable
    {
        public string Path { get; } = path;

        public static TempCsvFile Create(string content)
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");
            System.IO.File.WriteAllText(path, content);
            return new TempCsvFile(path);
        }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}
