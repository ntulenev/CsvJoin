using Microsoft.Extensions.Options;

using CsvJoin.Abstractions.Csv;

namespace CsvJoin.Configuration;

/// <summary>
/// Validates application settings before execution starts.
/// </summary>
internal sealed class AppSettingsValidator : IValidateOptions<AppSettings>
{
    private static readonly StringComparer AliasComparer = StringComparer.OrdinalIgnoreCase;
    private readonly ICsvJoinQueryParser _queryParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingsValidator"/> class.
    /// </summary>
    /// <param name="queryParser">The parser used to validate the configured query.</param>
    public AppSettingsValidator(ICsvJoinQueryParser queryParser)
    {
        ArgumentNullException.ThrowIfNull(queryParser);
        _queryParser = queryParser;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, AppSettings options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var errors = new List<string>();

        ValidateSources(options.Sources, errors);
        ValidateQuery(options.Query, options.Sources, errors);
        ValidateOutput(options.Output, errors);

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
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

    private void ValidateQuery(
        string? queryText,
        Dictionary<string, CsvSourceOptions> sources,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(queryText))
        {
            errors.Add("Query is missing in appsettings.json.");
            return;
        }

        try
        {
            var query = _queryParser.Parse(queryText);
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
        }
        catch (FormatException exception)
        {
            errors.Add(exception.Message);
        }
    }

    private static void ValidateOutput(OutputOptions? output, List<string> errors)
    {
        if (output is null)
        {
            errors.Add("Output section is missing in appsettings.json.");
            return;
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
}
