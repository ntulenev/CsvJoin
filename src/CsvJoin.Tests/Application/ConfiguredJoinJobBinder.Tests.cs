using FluentAssertions;

using CsvJoin.Application;
using CsvJoin.Configuration;
using CsvJoin.Csv;

namespace CsvJoin.Tests.Application;

public class ConfiguredJoinJobBinderTests
{
    [Fact(DisplayName = "ConfiguredJoinJobBinder Bind creates configured join job from settings.")]
    [Trait("Category", "Unit")]
    public void BindCreatesConfiguredJoinJobFromSettings()
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
        var sut = new ConfiguredJoinJobBinder(new CsvJoinQueryParser());

        // Act
        var result = sut.Bind(settings);

        // Assert
        result.Query.LeftAlias.Should().Be("left");
        result.LeftSource.FilePath.Should().Be(leftCsv.Path);
        result.RightSource.Delimiter.Should().Be(";");
        result.Output.Delimiter.Should().Be("|");
        result.Output.OpenResultAfterBuild.Should().BeFalse();
    }

    [Fact(DisplayName = "ConfiguredJoinJobBinder Bind throws when settings are null.")]
    [Trait("Category", "Unit")]
    public void BindThrowsWhenSettingsAreNull()
    {
        // Arrange
        var sut = new ConfiguredJoinJobBinder(new CsvJoinQueryParser());

        // Act
        Action action = () => _ = sut.Bind(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "ConfiguredJoinJobBinder Validate returns query alias error when source is missing.")]
    [Trait("Category", "Unit")]
    public void ValidateReturnsQueryAliasErrorWhenSourceIsMissing()
    {
        // Arrange
        var sut = new ConfiguredJoinJobBinder(new CsvJoinQueryParser());
        var settings = new AppSettings
        {
            Sources = new Dictionary<string, CsvSourceOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["left"] = new CsvSourceOptions { FilePath = "left.csv", Delimiter = "," },
                ["right"] = new CsvSourceOptions { FilePath = "right.csv", Delimiter = "," },
            },
            Query = "SELECT third.Id FROM third INNER JOIN right ON third.Id = right.Id",
            Output = new OutputOptions
            {
                ResultsDirectory = "results",
                Delimiter = ",",
                ConsoleMaxRows = 10,
                OpenResultAfterBuild = false,
            },
        };

        // Act
        var result = sut.Validate(settings);

        // Assert
        result.Should().Contain(x => x.Contains("third", StringComparison.Ordinal));
    }
}
