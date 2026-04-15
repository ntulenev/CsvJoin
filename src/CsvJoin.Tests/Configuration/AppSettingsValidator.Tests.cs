using FluentAssertions;

using Microsoft.Extensions.Options;

using CsvJoin.Configuration;
using CsvJoin.Csv;

namespace CsvJoin.Tests.Configuration;

public class AppSettingsValidatorTests
{
    [Fact(DisplayName = "AppSettingsValidator Validate throws when options are null.")]
    [Trait("Category", "Unit")]
    public void ValidateThrowsWhenOptionsAreNull()
    {
        // Arrange
        var sut = new AppSettingsValidator(new CsvJoinQueryParser());

        // Act
        Action action = () => _ = sut.Validate(Options.DefaultName, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact(DisplayName = "AppSettingsValidator Validate succeeds for valid settings.")]
    [Trait("Category", "Unit")]
    public void ValidateSucceedsForValidSettings()
    {
        // Arrange
        using var leftCsv = TempCsvFile.Create("Id,Name\n1,Alice\n");
        using var rightCsv = TempCsvFile.Create("Id,Status\n1,Active\n");
        var sut = new AppSettingsValidator(new CsvJoinQueryParser());
        var settings = CreateValidSettings(leftCsv.Path, rightCsv.Path);

        // Act
        var result = sut.Validate(Options.DefaultName, settings);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact(DisplayName = "AppSettingsValidator Validate fails when source file does not exist.")]
    [Trait("Category", "Unit")]
    public void ValidateFailsWhenSourceFileDoesNotExist()
    {
        // Arrange
        var sut = new AppSettingsValidator(new CsvJoinQueryParser());
        var settings = CreateValidSettings("missing-left.csv", "missing-right.csv");

        // Act
        var result = sut.Validate(Options.DefaultName, settings);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(x => x.Contains("left", StringComparison.OrdinalIgnoreCase));
        result.Failures.Should().Contain(x => x.Contains("right", StringComparison.OrdinalIgnoreCase));
    }

    [Fact(DisplayName = "AppSettingsValidator Validate fails when query is invalid.")]
    [Trait("Category", "Unit")]
    public void ValidateFailsWhenQueryIsInvalid()
    {
        // Arrange
        using var leftCsv = TempCsvFile.Create("Id\n1\n");
        using var rightCsv = TempCsvFile.Create("Id\n1\n");
        var sut = new AppSettingsValidator(new CsvJoinQueryParser());
        var settings = CreateValidSettings(
            leftCsv.Path,
            rightCsv.Path,
            query: "SELECT FROM broken");

        // Act
        var result = sut.Validate(Options.DefaultName, settings);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(x => x.Contains("Join query is invalid", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "AppSettingsValidator Validate fails when query references missing source alias.")]
    [Trait("Category", "Unit")]
    public void ValidateFailsWhenQueryReferencesMissingSourceAlias()
    {
        // Arrange
        using var leftCsv = TempCsvFile.Create("Id\n1\n");
        using var rightCsv = TempCsvFile.Create("Id\n1\n");
        var sut = new AppSettingsValidator(new CsvJoinQueryParser());
        var settings = CreateValidSettings(
            leftCsv.Path,
            rightCsv.Path,
            query: "SELECT other.Id FROM other INNER JOIN right ON other.Id = right.Id");

        // Act
        var result = sut.Validate(Options.DefaultName, settings);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(x => x.Contains("other", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "AppSettingsValidator Validate fails when output is invalid.")]
    [Trait("Category", "Unit")]
    public void ValidateFailsWhenOutputIsInvalid()
    {
        // Arrange
        using var leftCsv = TempCsvFile.Create("Id\n1\n");
        using var rightCsv = TempCsvFile.Create("Id\n1\n");
        var sut = new AppSettingsValidator(new CsvJoinQueryParser());
        var invalidOutput = new OutputOptions
        {
            ResultsDirectory = " ",
            Delimiter = " ",
            ConsoleMaxRows = -1,
        };
        var settings = CreateValidSettings(leftCsv.Path, rightCsv.Path, output: invalidOutput);

        // Act
        var result = sut.Validate(Options.DefaultName, settings);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain("Output:ResultsDirectory is required.");
        result.Failures.Should().Contain("Output:Delimiter is required.");
        result.Failures.Should().Contain("Output:ConsoleMaxRows must be zero or greater.");
    }

    private static AppSettings CreateValidSettings(
        string leftPath,
        string rightPath,
        string? query = null,
        OutputOptions? output = null)
    {
        return new AppSettings
        {
            Sources = new Dictionary<string, CsvSourceOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["left"] = new CsvSourceOptions { FilePath = leftPath, Delimiter = "," },
                ["right"] = new CsvSourceOptions { FilePath = rightPath, Delimiter = "," },
            },
            Query = query ?? "SELECT left.Id, right.Status FROM left LEFT JOIN right ON left.Id = right.Id",
            Output = output ?? new OutputOptions
            {
                ResultsDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N", System.Globalization.CultureInfo.InvariantCulture)),
                Delimiter = ",",
                ConsoleMaxRows = 10,
                OpenResultAfterBuild = false,
            },
        };
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
