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
        var joinKeys = new JoinKeyNormalizationSettings(true, true);
        var output = new JoinOutputSettings("results", ",", 10, true);

        // Act
        var sut = new ConfiguredJoinJob(query, "SELECT left.Id FROM left INNER JOIN right ON left.Id = right.Id", leftSource, rightSource, joinKeys, output);

        // Assert
        sut.Query.Should().BeSameAs(query);
        sut.QueryText.Should().Be("SELECT left.Id FROM left INNER JOIN right ON left.Id = right.Id");
        sut.LeftSource.Should().BeSameAs(leftSource);
        sut.RightSource.Should().BeSameAs(rightSource);
        sut.JoinKeys.Should().BeSameAs(joinKeys);
        sut.Output.Should().BeSameAs(output);
    }

    [Fact(DisplayName = "ConfiguredJoinJob ResolveSource returns right source for right side.")]
    [Trait("Category", "Unit")]
    public void ResolveSourceReturnsRightSourceForRightSide()
    {
        // Arrange
        var sut = new ConfiguredJoinJob(
            new CsvJoinQuery("left", "Id", "right", "Id", JoinType.Inner, []),
            "SELECT left.Id FROM left INNER JOIN right ON left.Id = right.Id",
            new ConfiguredCsvSource("left", new CsvSourceOptions { FilePath = "left.csv", Delimiter = "," }),
            new ConfiguredCsvSource("right", new CsvSourceOptions { FilePath = "right.csv", Delimiter = ";" }),
            new JoinKeyNormalizationSettings(false, false),
            new JoinOutputSettings("results", ",", 10, false));

        // Act
        var result = sut.ResolveSource(JoinSourceSide.Right);

        // Assert
        result.Alias.Should().Be("right");
    }
}
