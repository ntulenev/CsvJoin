using FluentAssertions;

using CsvJoin.Application;
using CsvJoin.Configuration;
using CsvJoin.Csv;

namespace CsvJoin.Tests.Application;

public class ConfiguredJoinJobBinderTests
{
    [Fact(DisplayName = "ConfiguredJoinJobBinder Bind returns configured join job for valid settings.")]
    [Trait("Category", "Unit")]
    public void BindReturnsConfiguredJoinJobForValidSettings()
    {
        // Arrange
        using var leftCsv = TempCsvFile.Create("Id\n1\n");
        using var rightCsv = TempCsvFile.Create("Id\n1\n");
        var settings = new AppSettings
        {
            Sources = new Dictionary<string, CsvSourceOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["left"] = new CsvSourceOptions { FilePath = leftCsv.Path, Delimiter = "," },
                ["right"] = new CsvSourceOptions { FilePath = rightCsv.Path, Delimiter = ";" },
            },
            Query = "SELECT left.Id, right.Status FROM left LEFT JOIN right ON left.Id = right.Id",
            Output = new OutputOptions
            {
                ResultsDirectory = "results",
                Delimiter = "|",
                ConsoleMaxRows = 5,
                OpenResultAfterBuild = false,
            },
        };
        var sut = new ConfiguredJoinJobBinder(new ConfiguredJoinJobSettingsBinder(new CsvJoinQueryParser()));

        // Act
        var result = sut.Bind(settings);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Job.Should().NotBeNull();
        result.Job!.Query.LeftAlias.Should().Be("left");
        result.Job.LeftSource.FilePath.Should().Be(leftCsv.Path);
        result.Job.RightSource.Delimiter.Should().Be(";");
        result.Job.Output.Delimiter.Should().Be("|");
        result.Job.Output.OpenResultAfterBuild.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "ConfiguredJoinJobBinder Bind throws when settings are null.")]
    [Trait("Category", "Unit")]
    public void BindThrowsWhenSettingsAreNull()
    {
        // Arrange
        var sut = new ConfiguredJoinJobBinder(new ConfiguredJoinJobSettingsBinder(new CsvJoinQueryParser()));

        // Act
        Action action = () => _ = sut.Bind(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "ConfiguredJoinJobBinder Bind returns file error when source file does not exist.")]
    [Trait("Category", "Unit")]
    public void BindReturnsFileErrorWhenSourceFileDoesNotExist()
    {
        // Arrange
        var sut = new ConfiguredJoinJobBinder(new ConfiguredJoinJobSettingsBinder(new CsvJoinQueryParser()));
        var settings = new AppSettings
        {
            Sources = new Dictionary<string, CsvSourceOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["left"] = new CsvSourceOptions { FilePath = "left.csv", Delimiter = "," },
                ["right"] = new CsvSourceOptions { FilePath = "right.csv", Delimiter = "," },
            },
            Query = "SELECT left.Id, right.Status FROM left INNER JOIN right ON left.Id = right.Id",
            Output = new OutputOptions
            {
                ResultsDirectory = "results",
                Delimiter = ",",
                ConsoleMaxRows = 10,
                OpenResultAfterBuild = false,
            },
        };

        // Act
        var result = sut.Bind(settings);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(x => x.Contains("left.csv", StringComparison.Ordinal));
        result.Errors.Should().Contain(x => x.Contains("right.csv", StringComparison.Ordinal));
    }
}
