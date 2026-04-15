using CsvJoin.Abstractions.Application;
using CsvJoin.Abstractions.Csv;
using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Application;

/// <summary>
/// Binds application settings to executable join jobs.
/// </summary>
internal sealed class ConfiguredJoinJobBinder : IConfiguredJoinJobBinder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfiguredJoinJobBinder"/> class.
    /// </summary>
    /// <param name="queryParser">The parser used to translate the configured query.</param>
    public ConfiguredJoinJobBinder(ICsvJoinQueryParser queryParser)
    {
        ArgumentNullException.ThrowIfNull(queryParser);
        _queryParser = queryParser;
    }

    /// <inheritdoc />
    public ConfiguredJoinJob Bind(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var validationErrors = BindCore(settings, out var job);
        if (validationErrors.Count > 0 || job is null)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, validationErrors));
        }

        return job;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> Validate(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return BindCore(settings, out _);
    }

    private List<string> BindCore(AppSettings settings, out ConfiguredJoinJob? job)
    {
        var errors = new List<string>();

        ValidateSources(settings.Sources, errors);
        var query = ValidateAndParseQuery(settings.Query, settings.Sources, errors);
        var output = ValidateOutput(settings.Output, errors);

        if (query is null || output is null)
        {
            job = null;
            return errors;
        }

        var leftSource = ResolveSource(settings.Sources, query.LeftAlias, errors);
        var rightSource = ResolveSource(settings.Sources, query.RightAlias, errors);
        if (leftSource is null || rightSource is null)
        {
            job = null;
            return errors;
        }

        job = new ConfiguredJoinJob(
            query,
            new ConfiguredCsvSource(query.LeftAlias, leftSource),
            new ConfiguredCsvSource(query.RightAlias, rightSource),
            output);

        return errors;
    }

    private static void ValidateSources(Dictionary<string, CsvSourceOptions>? sources, List<string> errors)
    {
        if (sources is null)
        {
            errors.Add("Sources section is missing in appsettings.json.");
            return;
        }

        if (sources.Count != 2)
        {
            errors.Add("Exactly two CSV sources must be configured in Sources.");
        }

        foreach (var (alias, source) in sources)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                errors.Add("Source alias must not be empty.");
                continue;
            }

            if (!IsValidAlias(alias))
            {
                errors.Add($"Source alias '{alias}' is invalid. Use letters, digits, and underscore only.");
            }

            if (source is null)
            {
                errors.Add($"Source '{alias}' is missing its configuration block.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(source.FilePath))
            {
                errors.Add($"Sources:{alias}:FilePath is required.");
            }
            else if (!File.Exists(source.FilePath))
            {
                errors.Add($"CSV file not found for source '{alias}': {source.FilePath}");
            }

            if (string.IsNullOrWhiteSpace(source.Delimiter))
            {
                errors.Add($"Sources:{alias}:Delimiter is required.");
            }
        }
    }

    private CsvJoinQuery? ValidateAndParseQuery(
        string? queryText,
        Dictionary<string, CsvSourceOptions>? sources,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(queryText))
        {
            errors.Add("Query is missing in appsettings.json.");
            return null;
        }

        try
        {
            var query = _queryParser.Parse(queryText);
            if (sources is null)
            {
                return query;
            }

            if (!sources.ContainsKey(query.LeftAlias))
            {
                errors.Add($"Query references missing source alias '{query.LeftAlias}'.");
            }

            if (!sources.ContainsKey(query.RightAlias))
            {
                errors.Add($"Query references missing source alias '{query.RightAlias}'.");
            }

            if (AliasComparer.Equals(query.LeftAlias, query.RightAlias))
            {
                errors.Add("Query must reference two different source aliases.");
            }

            return query;
        }
        catch (FormatException exception)
        {
            errors.Add(exception.Message);
            return null;
        }
    }

    private static JoinOutputSettings? ValidateOutput(OutputOptions? output, List<string> errors)
    {
        if (output is null)
        {
            errors.Add("Output section is missing in appsettings.json.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(output.ResultsDirectory))
        {
            errors.Add("Output:ResultsDirectory is required.");
        }

        if (string.IsNullOrWhiteSpace(output.Delimiter))
        {
            errors.Add("Output:Delimiter is required.");
        }

        if (output.ConsoleMaxRows < 0)
        {
            errors.Add("Output:ConsoleMaxRows must be zero or greater.");
        }

        if (errors.Count > 0)
        {
            return null;
        }

        return new JoinOutputSettings(
            output.ResultsDirectory,
            output.Delimiter,
            output.ConsoleMaxRows,
            output.OpenResultAfterBuild);
    }

    private static CsvSourceOptions? ResolveSource(
        Dictionary<string, CsvSourceOptions>? sources,
        string alias,
        List<string> errors)
    {
        if (sources is not null && sources.TryGetValue(alias, out var source) && source is not null)
        {
            return source;
        }

        errors.Add($"Query references missing source alias '{alias}'.");
        return null;
    }

    private static bool IsValidAlias(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            return false;
        }

        if (!(char.IsLetter(alias[0]) || alias[0] == '_'))
        {
            return false;
        }

        for (var i = 1; i < alias.Length; i++)
        {
            var current = alias[i];
            if (!(char.IsLetterOrDigit(current) || current == '_'))
            {
                return false;
            }
        }

        return true;
    }

    private static readonly StringComparer AliasComparer = StringComparer.OrdinalIgnoreCase;
    private readonly ICsvJoinQueryParser _queryParser;
}
