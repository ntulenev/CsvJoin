using CsvJoin.Abstractions.Application;
using CsvJoin.Abstractions.Csv;
using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Application;

internal sealed class ConfiguredJoinJobFactory : IConfiguredJoinJobFactory
{
    private readonly ICsvJoinQueryParser _queryParser;

    public ConfiguredJoinJobFactory(ICsvJoinQueryParser queryParser)
    {
        ArgumentNullException.ThrowIfNull(queryParser);
        _queryParser = queryParser;
    }

    public ConfiguredJoinJob Create(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var query = _queryParser.Parse(settings.Query);
        var leftSource = ResolveSource(settings, query.LeftAlias);
        var rightSource = ResolveSource(settings, query.RightAlias);
        var output = new JoinOutputSettings(
            settings.Output.ResultsDirectory,
            settings.Output.Delimiter,
            settings.Output.ConsoleMaxRows,
            settings.Output.OpenResultAfterBuild);

        return new ConfiguredJoinJob(
            query,
            new ConfiguredCsvSource(query.LeftAlias, leftSource),
            new ConfiguredCsvSource(query.RightAlias, rightSource),
            output);
    }

    private static CsvSourceOptions ResolveSource(AppSettings settings, string alias)
    {
        if (settings.Sources.TryGetValue(alias, out var source))
        {
            return source;
        }

        throw new InvalidOperationException($"CSV source '{alias}' is missing in configuration.");
    }
}
