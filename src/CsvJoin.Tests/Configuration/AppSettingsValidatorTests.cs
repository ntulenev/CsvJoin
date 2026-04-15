using Microsoft.Extensions.Options;

using CsvJoin.Configuration;
using CsvJoin.Csv;

namespace CsvJoin.Tests.Configuration;

public sealed class AppSettingsValidatorTests
{
    [Fact]
    public void ValidateWhenSourceFileIsMissingReturnsFailure()
    {
        var parser = new CsvJoinQueryParser();
        var validator = new AppSettingsValidator(parser);
        var options = new AppSettings
        {
            Sources = new Dictionary<string, CsvSourceOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["left"] = new CsvSourceOptions { FilePath = "C:\\missing-left.csv" },
                ["right"] = new CsvSourceOptions { FilePath = "C:\\missing-right.csv" },
            },
            Query = "SELECT left.Id FROM left INNER JOIN right ON left.Id = right.Id",
            Output = new OutputOptions(),
        };

        var result = validator.Validate(Options.DefaultName, options);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Failures!, failure => failure.Contains("CSV file not found", StringComparison.Ordinal));
    }
}
