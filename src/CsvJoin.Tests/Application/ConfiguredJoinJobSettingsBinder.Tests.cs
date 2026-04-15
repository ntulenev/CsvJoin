using FluentAssertions;

using CsvJoin.Application;
using CsvJoin.Configuration;
using CsvJoin.Csv;

namespace CsvJoin.Tests.Application;

public class ConfiguredJoinJobSettingsBinderTests
{
    [Fact(DisplayName = "ConfiguredJoinJobSettingsBinder Bind creates configured join job without checking file existence.")]
    [Trait("Category", "Unit")]
    public void BindCreatesConfiguredJoinJobWithoutCheckingFileExistence()
    {
        // Arrange
        var settings = new AppSettings
        {
            Sources = new Dictionary<string, CsvSourceOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["left"] = new CsvSourceOptions { FilePath = "missing-left.csv", Delimiter = "," },
                ["right"] = new CsvSourceOptions { FilePath = "missing-right.csv", Delimiter = ";" },
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
        var sut = new ConfiguredJoinJobSettingsBinder(new CsvJoinQueryParser());

        // Act
        var result = sut.Bind(settings);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Job.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "ConfiguredJoinJobSettingsBinder Bind returns query alias error when source is missing.")]
    [Trait("Category", "Unit")]
    public void BindReturnsQueryAliasErrorWhenSourceIsMissing()
    {
        // Arrange
        var sut = new ConfiguredJoinJobSettingsBinder(new CsvJoinQueryParser());
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
        var result = sut.Bind(settings);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Job.Should().BeNull();
        result.Errors.Should().Contain(x => x.Contains("third", StringComparison.Ordinal));
    }
}
