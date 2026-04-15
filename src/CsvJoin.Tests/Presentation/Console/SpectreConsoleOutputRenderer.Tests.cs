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
        var job = CreateJob();

        // Act
        var exception = Record.Exception(() => sut.RenderHeader(job));

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = "SpectreConsoleOutputRenderer RenderHeader throws when job is null.")]
    [Trait("Category", "Unit")]
    public void RenderHeaderThrowsWhenJobIsNull()
    {
        // Arrange
        var sut = new SpectreConsoleOutputRenderer();

        // Act
        Action action = () => sut.RenderHeader(null!);

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

    private static ConfiguredJoinJob CreateJob()
    {
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, [new SelectColumn("left", "Id", "Id")]);
        return new ConfiguredJoinJob(
            query,
            new ConfiguredCsvSource("left", new CsvSourceOptions { FilePath = "left.csv", Delimiter = "," }),
            new ConfiguredCsvSource("right", new CsvSourceOptions { FilePath = "right.csv", Delimiter = "," }),
            new JoinOutputSettings("results", ",", 10, false));
    }

    private static CsvJoinResult CreateResult()
    {
        var headers = new[] { "Id" };
        var rows = new[] { (IReadOnlyList<string?>)new string?[] { "1" } };
        return new CsvJoinResult("left.csv", "right.csv", headers, rows);
    }
}
