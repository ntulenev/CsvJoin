using FluentAssertions;

using CsvJoin.Application;
using CsvJoin.Configuration;
using CsvJoin.Csv;

namespace CsvJoin.Tests.Application;

public class ConfiguredJoinJobFactoryTests
{
    [Fact(DisplayName = "ConfiguredJoinJobFactory creates configured join job from settings.")]
    [Trait("Category", "Unit")]
    public void CreateCreatesConfiguredJoinJobFromSettings()
    {
        // Arrange
        var settings = new AppSettings
        {
            Sources = new Dictionary<string, CsvSourceOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["left"] = new CsvSourceOptions { FilePath = "left.csv", Delimiter = "," },
                ["right"] = new CsvSourceOptions { FilePath = "right.csv", Delimiter = ";" },
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
        var sut = new ConfiguredJoinJobFactory(new CsvJoinQueryParser());

        // Act
        var result = sut.Create(settings);

        // Assert
        result.Query.LeftAlias.Should().Be("left");
        result.LeftSource.FilePath.Should().Be("left.csv");
        result.RightSource.Delimiter.Should().Be(";");
        result.Output.Delimiter.Should().Be("|");
        result.Output.OpenResultAfterBuild.Should().BeFalse();
    }

    [Fact(DisplayName = "ConfiguredJoinJobFactory throws when settings are null.")]
    [Trait("Category", "Unit")]
    public void CreateThrowsWhenSettingsAreNull()
    {
        // Arrange
        var sut = new ConfiguredJoinJobFactory(new CsvJoinQueryParser());

        // Act
        Action action = () => _ = sut.Create(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }
}
