using FluentAssertions;

using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class JoinOutputFileTests
{
    [Fact(DisplayName = "JoinOutputFile stores constructor values.")]
    [Trait("Category", "Unit")]
    public void JoinOutputFileStoresConstructorValues()
    {
        // Arrange
        // Act
        var sut = new JoinOutputFile("joined.csv", 42);

        // Assert
        sut.FilePath.Should().Be("joined.csv");
        sut.RowCount.Should().Be(42);
    }
}
