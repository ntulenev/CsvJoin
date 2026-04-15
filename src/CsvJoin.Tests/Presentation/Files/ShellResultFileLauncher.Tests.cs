using FluentAssertions;

using CsvJoin.Presentation.Files;

namespace CsvJoin.Tests.Presentation.Files;

public class ShellResultFileLauncherTests
{
    [Fact(DisplayName = "ShellResultFileLauncher TryOpen validates file path.")]
    [Trait("Category", "Unit")]
    public void TryOpenValidatesFilePath()
    {
        // Arrange
        var sut = new ShellResultFileLauncher();

        // Act
        Action action = () => _ = sut.TryOpen(" ", out _);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "ShellResultFileLauncher TryOpen returns false for missing file.")]
    [Trait("Category", "Unit")]
    public void TryOpenReturnsFalseForMissingFile()
    {
        // Arrange
        var sut = new ShellResultFileLauncher();

        // Act
        var result = sut.TryOpen("Z:\\this-file-does-not-exist-123456789.txt", out var errorMessage);

        // Assert
        result.Should().BeFalse();
        errorMessage.Should().NotBeNullOrWhiteSpace();
    }
}
