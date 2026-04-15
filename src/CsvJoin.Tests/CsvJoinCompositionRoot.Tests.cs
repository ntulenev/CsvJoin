using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CsvJoin.Abstractions.Application;

namespace CsvJoin.Tests;

public class CsvJoinCompositionRootTests
{
    [Fact(DisplayName = "AddCsvJoinServices resolves application for valid configuration.")]
    [Trait("Category", "Unit")]
    public void AddCsvJoinServicesResolvesApplicationForValidConfiguration()
    {
        // Arrange
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var leftPath = Path.Combine(tempDirectory, "left.csv");
            var rightPath = Path.Combine(tempDirectory, "right.csv");

            File.WriteAllText(leftPath, "Id,Name\r\n1,Alice\r\n");
            File.WriteAllText(rightPath, "Id,Status\r\n1,Active\r\n");

            var configuration = CreateConfiguration(leftPath, rightPath, Path.Combine(tempDirectory, "results"));
            var services = new ServiceCollection();
            CsvJoinCompositionRoot.AddCsvJoinServices(services, configuration);

            using var serviceProvider = services.BuildServiceProvider();

            // Act
            var application = serviceProvider.GetRequiredService<ICsvJoinApplication>();

            // Assert
            application.Should().NotBeNull();
        }
        finally
        {
            DeleteTemporaryDirectory(tempDirectory);
        }
    }

    [Fact(DisplayName = "AddCsvJoinServices throws when source file is missing.")]
    [Trait("Category", "Unit")]
    public void AddCsvJoinServicesThrowsWhenSourceFileIsMissing()
    {
        // Arrange
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var leftPath = Path.Combine(tempDirectory, "left.csv");
            File.WriteAllText(leftPath, "Id,Name\r\n1,Alice\r\n");

            var missingRightPath = Path.Combine(tempDirectory, "missing-right.csv");
            var configuration = CreateConfiguration(leftPath, missingRightPath, Path.Combine(tempDirectory, "results"));
            var services = new ServiceCollection();
            CsvJoinCompositionRoot.AddCsvJoinServices(services, configuration);

            using var serviceProvider = services.BuildServiceProvider();

            // Act
            Action action = () => _ = serviceProvider.GetRequiredService<ICsvJoinApplication>();

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*missing-right.csv*");
        }
        finally
        {
            DeleteTemporaryDirectory(tempDirectory);
        }
    }

    private static IConfiguration CreateConfiguration(string leftPath, string rightPath, string resultsDirectory)
    {
        var values = new Dictionary<string, string?>
        {
            ["Sources:left:FilePath"] = leftPath,
            ["Sources:left:Delimiter"] = ",",
            ["Sources:right:FilePath"] = rightPath,
            ["Sources:right:Delimiter"] = ",",
            ["Query"] = "SELECT left.Id, right.Status FROM left INNER JOIN right ON left.Id = right.Id",
            ["Output:ResultsDirectory"] = resultsDirectory,
            ["Output:Delimiter"] = ",",
            ["Output:ConsoleMaxRows"] = "10",
            ["Output:OpenResultAfterBuild"] = "false",
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
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
