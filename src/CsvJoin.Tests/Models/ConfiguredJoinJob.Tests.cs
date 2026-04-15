using FluentAssertions;

using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Tests.Models;

public class ConfiguredJoinJobTests
{
    [Fact(DisplayName = "ConfiguredJoinJob stores constructor values.")]
    [Trait("Category", "Unit")]
    public void ConfiguredJoinJobStoresConstructorValues()
    {
        // Arrange
        var query = new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, []);
        var leftSource = new ConfiguredCsvSource("left", new CsvSourceOptions { FilePath = "left.csv", Delimiter = "," });
        var rightSource = new ConfiguredCsvSource("right", new CsvSourceOptions { FilePath = "right.csv", Delimiter = "," });
        var output = new JoinOutputSettings("results", ",", 10, true);

        // Act
        var sut = new ConfiguredJoinJob(query, leftSource, rightSource, output);

        // Assert
        sut.Query.Should().BeSameAs(query);
        sut.LeftSource.Should().BeSameAs(leftSource);
        sut.RightSource.Should().BeSameAs(rightSource);
        sut.Output.Should().BeSameAs(output);
    }

    [Fact(DisplayName = "ConfiguredJoinJob ResolveSource returns right source for right side.")]
    [Trait("Category", "Unit")]
    public void ResolveSourceReturnsRightSourceForRightSide()
    {
        // Arrange
        var sut = new ConfiguredJoinJob(
            new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, []),
            new ConfiguredCsvSource("left", new CsvSourceOptions { FilePath = "left.csv", Delimiter = "," }),
            new ConfiguredCsvSource("right", new CsvSourceOptions { FilePath = "right.csv", Delimiter = ";" }),
            new JoinOutputSettings("results", ",", 10, false));

        // Act
        var result = sut.ResolveSource(JoinSourceSide.Right);

        // Assert
        result.Alias.Should().Be("right");
    }
}
