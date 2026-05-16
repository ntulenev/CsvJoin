using CsvJoin.Abstractions.Application;
using CsvJoin.Abstractions.Csv;
using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Application;

/// <summary>
/// Binds application settings to executable join jobs using pure configuration rules.
/// </summary>
internal sealed class ConfiguredJoinJobSettingsBinder : IConfiguredJoinJobSettingsBinder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfiguredJoinJobSettingsBinder"/> class.
    /// </summary>
    /// <param name="queryParser">The parser used to translate the configured query.</param>
    public ConfiguredJoinJobSettingsBinder(ICsvJoinQueryParser queryParser)
    {
        ArgumentNullException.ThrowIfNull(queryParser);
        _queryParser = queryParser;
    }

    /// <inheritdoc />
    public ConfiguredJoinJobBindingResult Bind(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var errors = new List<string>();

        ValidateSources(settings.Sources, errors);
        var query = ValidateAndParseQuery(settings.Query, settings.Sources, errors);
        var columnTypes = ValidateColumnTypes(settings.ColumnTypes, settings.Sources, errors);
        var output = ValidateOutput(settings.Output, errors);

        if (query is null || output is null)
        {
            return new ConfiguredJoinJobBindingResult(null, errors);
        }

        var leftSource = ResolveSource(settings.Sources, query.LeftAlias, errors);
        var rightSource = ResolveSource(settings.Sources, query.RightAlias, errors);
        if (leftSource is null || rightSource is null)
        {
            return new ConfiguredJoinJobBindingResult(null, errors);
        }

        var job = new ConfiguredJoinJob(
            query,
            settings.Query,
            new ConfiguredCsvSource(query.LeftAlias, leftSource),
            new ConfiguredCsvSource(query.RightAlias, rightSource),
            new JoinKeyNormalizationSettings(settings.JoinKeys.TrimWhitespace, settings.JoinKeys.IgnoreCase),
            new ColumnTypeRegistry(columnTypes),
            output);

        return new ConfiguredJoinJobBindingResult(job, errors);
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

            if (string.IsNullOrWhiteSpace(source.Delimiter))
            {
                errors.Add($"Sources:{alias}:Delimiter is required.");
            }

            if (string.IsNullOrWhiteSpace(source.Encoding))
            {
                errors.Add($"Sources:{alias}:Encoding is required.");
            }

            if (string.IsNullOrWhiteSpace(source.Quote))
            {
                errors.Add($"Sources:{alias}:Quote is required.");
            }
            else if (source.Quote.Length != 1)
            {
                errors.Add($"Sources:{alias}:Quote must be exactly one character.");
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

    private static Dictionary<string, ColumnDataType> ValidateColumnTypes(
        Dictionary<string, string>? columnTypes,
        Dictionary<string, CsvSourceOptions>? sources,
        List<string> errors)
    {
        var result = new Dictionary<string, ColumnDataType>(StringComparer.OrdinalIgnoreCase);
        if (columnTypes is null)
        {
            return result;
        }

        foreach (var (columnReference, rawDataType) in columnTypes)
        {
            if (!TryParseColumnReference(columnReference, out var alias, out var field))
            {
                errors.Add($"ColumnTypes key '{columnReference}' is invalid. Use alias.Field.");
                continue;
            }

            if (sources is not null && !sources.ContainsKey(alias))
            {
                errors.Add($"ColumnTypes key '{columnReference}' references missing source alias '{alias}'.");
            }

            if (!TryParseColumnDataType(rawDataType, out var dataType))
            {
                errors.Add($"ColumnTypes:{columnReference} has unsupported type '{rawDataType}'. Use text, number, or date.");
                continue;
            }

            result[$"{alias}.{field}"] = dataType;
        }

        return result;
    }

    private static bool TryParseColumnReference(string value, out string alias, out string field)
    {
        alias = string.Empty;
        field = string.Empty;

        var separatorIndex = value.AsSpan().IndexOf('.');
        if (separatorIndex <= 0 || separatorIndex == value.Length - 1)
        {
            return false;
        }

        alias = value[..separatorIndex];
        field = UnwrapBracketedField(value[(separatorIndex + 1)..]);
        return IsValidAlias(alias) && !string.IsNullOrWhiteSpace(field);
    }

    private static string UnwrapBracketedField(string field)
    {
        return field.Length >= 2 && field[0] == '[' && field[^1] == ']'
            ? field[1..^1]
            : field;
    }

    private static bool TryParseColumnDataType(string value, out ColumnDataType dataType)
    {
        dataType = ColumnDataType.Text;
        if (string.Equals(value, "text", StringComparison.OrdinalIgnoreCase))
        {
            dataType = ColumnDataType.Text;
            return true;
        }

        if (string.Equals(value, "number", StringComparison.OrdinalIgnoreCase))
        {
            dataType = ColumnDataType.Number;
            return true;
        }

        if (string.Equals(value, "date", StringComparison.OrdinalIgnoreCase))
        {
            dataType = ColumnDataType.Date;
            return true;
        }

        return false;
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
