using FluentAssertions;

using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class JoinOutputColumnTests
{
    [Fact(DisplayName = "JoinOutputColumn stores constructor values.")]
    [Trait("Category", "Unit")]
    public void JoinOutputColumnStoresConstructorValues()
    {
        // Arrange
        // Act
        var sut = new JoinOutputColumn("left", "Id", "OutputId");

        // Assert
        sut.SourceAlias.Should().Be("left");
        sut.SourceField.Should().Be("Id");
        sut.OutputField.Should().Be("OutputId");
    }
}
