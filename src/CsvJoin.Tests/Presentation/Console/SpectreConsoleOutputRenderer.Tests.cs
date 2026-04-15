using FluentAssertions;

using CsvJoin.Configuration;
using CsvJoin.Models;
using CsvJoin.Presentation.Console;

namespace CsvJoin.Tests.Presentation.Console;

public class SpectreConsoleOutputRendererTests
{
    [Fact(DisplayName = "SpectreConsoleOutputRenderer RenderHeader executes for valid input.")]
    [Trait("Category", "Unit")]
    public void RenderHeaderExecutesForValidInput()
    {
        // Arrange
        var sut = new SpectreConsoleOutputRenderer();
        var settings = CreateSettings();
        var query = CreateQuery();

        // Act
        var exception = Record.Exception(() => sut.RenderHeader(settings, query));

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = "SpectreConsoleOutputRenderer RenderHeader throws when settings are null.")]
    [Trait("Category", "Unit")]
    public void RenderHeaderThrowsWhenSettingsAreNull()
    {
        // Arrange
        var sut = new SpectreConsoleOutputRenderer();
        var query = CreateQuery();

        // Act
        Action action = () => sut.RenderHeader(null!, query);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "SpectreConsoleOutputRenderer RenderHeader throws when query is null.")]
    [Trait("Category", "Unit")]
    public void RenderHeaderThrowsWhenQueryIsNull()
    {
        // Arrange
        var sut = new SpectreConsoleOutputRenderer();
        var settings = CreateSettings();

        // Act
        Action action = () => sut.RenderHeader(settings, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "SpectreConsoleOutputRenderer RenderResult executes for valid input.")]
    [Trait("Category", "Unit")]
    public void RenderResultExecutesForValidInput()
    {
        // Arrange
        var sut = new SpectreConsoleOutputRenderer();
        var result = CreateResult();

        // Act
        var exception = Record.Exception(() => sut.RenderResult(result, 10));

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = "SpectreConsoleOutputRenderer RenderResult throws when result is null.")]
    [Trait("Category", "Unit")]
    public void RenderResultThrowsWhenResultIsNull()
    {
        // Arrange
        var sut = new SpectreConsoleOutputRenderer();

        // Act
        Action action = () => sut.RenderResult(null!, 0);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "SpectreConsoleOutputRenderer PrintFileSaved executes for valid input.")]
    [Trait("Category", "Unit")]
    public void PrintFileSavedExecutesForValidInput()
    {
        // Arrange
        var sut = new SpectreConsoleOutputRenderer();
        var outputFile = new JoinOutputFile("joined.csv", 1);

        // Act
        var exception = Record.Exception(() => sut.PrintFileSaved(outputFile));

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = "SpectreConsoleOutputRenderer PrintFileSaved throws when output file is null.")]
    [Trait("Category", "Unit")]
    public void PrintFileSavedThrowsWhenOutputFileIsNull()
    {
        // Arrange
        var sut = new SpectreConsoleOutputRenderer();

        // Act
        Action action = () => sut.PrintFileSaved(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "SpectreConsoleOutputRenderer PrintFileOpened executes for valid input.")]
    [Trait("Category", "Unit")]
    public void PrintFileOpenedExecutesForValidInput()
    {
        // Arrange
        var sut = new SpectreConsoleOutputRenderer();

        // Act
        var exception = Record.Exception(() => sut.PrintFileOpened("joined.csv"));

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = "SpectreConsoleOutputRenderer PrintFileOpened throws when path is empty.")]
    [Trait("Category", "Unit")]
    public void PrintFileOpenedThrowsWhenPathIsEmpty()
    {
        // Arrange
        var sut = new SpectreConsoleOutputRenderer();

        // Act
        Action action = () => sut.PrintFileOpened(" ");

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "SpectreConsoleOutputRenderer PrintFileOpenWarning executes for valid input.")]
    [Trait("Category", "Unit")]
    public void PrintFileOpenWarningExecutesForValidInput()
    {
        // Arrange
        var sut = new SpectreConsoleOutputRenderer();

        // Act
        var exception = Record.Exception(() => sut.PrintFileOpenWarning("warning"));

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = "SpectreConsoleOutputRenderer PrintFileOpenWarning throws when message is empty.")]
    [Trait("Category", "Unit")]
    public void PrintFileOpenWarningThrowsWhenMessageIsEmpty()
    {
        // Arrange
        var sut = new SpectreConsoleOutputRenderer();

        // Act
        Action action = () => sut.PrintFileOpenWarning(" ");

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    private static AppSettings CreateSettings()
    {
        return new AppSettings
        {
            Sources = new Dictionary<string, CsvSourceOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["left"] = new CsvSourceOptions { FilePath = "left.csv" },
                ["right"] = new CsvSourceOptions { FilePath = "right.csv" },
            },
            Query = "SELECT left.Id FROM left INNER JOIN right ON left.Id = right.Id",
            Output = new OutputOptions(),
        };
    }

    private static CsvJoinQuery CreateQuery()
    {
        var columns = new[] { new SelectColumn("left", "Id", "Id") };
        return new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, columns);
    }

    private static CsvJoinResult CreateResult()
    {
        var headers = new[] { "Id" };
        var rows = new[] { (IReadOnlyList<string?>)new string?[] { "1" } };
        return new CsvJoinResult("left.csv", "right.csv", headers, rows);
    }
}
