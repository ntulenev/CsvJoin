using FluentAssertions;

using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class CsvDataRowTests
{
    [Fact(DisplayName = "CsvDataRow stores constructor values.")]
    [Trait("Category", "Unit")]
    public void CsvDataRowStoresConstructorValues()
    {
        // Arrange
        var values = new Dictionary<string, string?> { ["Id"] = "1" };

        // Act
        var sut = new CsvDataRow(5, values);

        // Assert
        sut.Index.Should().Be(5);
        sut.Values.Should().BeSameAs(values);
    }
}
