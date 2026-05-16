using System.Globalization;
using System.Text.RegularExpressions;

using CsvJoin.Abstractions.Csv;
using CsvJoin.Csv.Syntax;
using CsvJoin.Models;

namespace CsvJoin.Csv;

/// <summary>
/// Parses SQL-like join queries into <see cref="CsvJoinQuery"/> instances.
/// </summary>
internal sealed partial class CsvJoinQueryParser : ICsvJoinQueryParser
{
    /// <inheritdoc />
    public CsvJoinQuery Parse(string queryText)
    {
        if (string.IsNullOrWhiteSpace(queryText))
        {
            throw new FormatException("Join query must not be empty.");
        }

        var match = QueryPattern().Match(queryText.Trim());
        if (!match.Success)
        {
            throw new FormatException(
                "Join query is invalid. Expected format: SELECT left.Id, right.Name FROM left INNER JOIN right ON left.Id = right.Id");
        }

        var selectSegment = match.Groups["select"].Value;
        var parsedSelect = ParseSelectSegment(selectSegment);
        var parsedTail = ParseQueryTail(match.Groups["tail"].Value);
        if (parsedSelect.Limit is not null && parsedTail.Limit is not null)
        {
            throw new FormatException("Use either TOP or LIMIT, not both.");
        }

        var syntax = new JoinQuerySyntax(
            ParseSelectColumns(parsedSelect.SelectSegment),
            match.Groups["leftAlias"].Value,
            match.Groups["rightAlias"].Value,
            ParseJoinType(match.Groups["joinType"].Value),
            new JoinConditionSyntax(
                FieldReferenceParser.Parse(parsedTail.LeftReference, allowWildcard: false),
                FieldReferenceParser.Parse(parsedTail.RightReference, allowWildcard: false)));

        var query = syntax.ToDomainQuery();
        return query with
        {
            SourceFilters = parsedTail.SourceFilters,
            IsDistinct = parsedSelect.IsDistinct,
            OrderByColumns = parsedTail.OrderByColumns,
            Limit = parsedSelect.Limit ?? parsedTail.Limit,
        };
    }

    private static JoinType ParseJoinType(string rawJoinType) => rawJoinType.ToUpperInvariant() switch
    {
        "INNER" => JoinType.Inner,
        "LEFT" => JoinType.Left,
        "RIGHT" => JoinType.Right,
        "FULL" => JoinType.Full,
        _ => throw new FormatException($"Unsupported join type '{rawJoinType}'."),
    };

    private static SelectProjectionSyntax[] ParseSelectColumns(string selectSegment) =>
        SelectClauseTokenizer.Split(selectSegment)
            .Select(SelectProjectionParser.Parse)
            .ToArray();

    private static ParsedSelectSegment ParseSelectSegment(string selectSegment)
    {
        var remaining = selectSegment.Trim();
        var isDistinct = false;
        int? limit = null;

        if (remaining.StartsWith("DISTINCT ", StringComparison.OrdinalIgnoreCase))
        {
            isDistinct = true;
            remaining = remaining["DISTINCT ".Length..].TrimStart();
        }

        var topMatch = TopPattern().Match(remaining);
        if (topMatch.Success)
        {
            limit = ParseLimit(topMatch.Groups["limit"].Value);
            remaining = remaining[topMatch.Length..].TrimStart();
        }

        if (string.IsNullOrWhiteSpace(remaining))
        {
            throw new FormatException("SELECT clause must contain at least one column expression.");
        }

        return new ParsedSelectSegment(remaining, isDistinct, limit);
    }

    private static ParsedQueryTail ParseQueryTail(string queryTail)
    {
        var remaining = queryTail.Trim();
        int? limit = null;
        var orderByColumns = Array.Empty<OrderByColumn>();
        var sourceFilters = Array.Empty<SourceFilter>();

        var limitMatch = LimitPattern().Match(remaining);
        if (limitMatch.Success)
        {
            limit = ParseLimit(limitMatch.Groups["limit"].Value);
            remaining = remaining[..limitMatch.Index].TrimEnd();
        }

        var orderByMatch = OrderByPattern().Match(remaining);
        if (orderByMatch.Success)
        {
            orderByColumns = ParseOrderByColumns(orderByMatch.Groups["orderBy"].Value);
            remaining = remaining[..orderByMatch.Index].TrimEnd();
        }

        var whereMatch = WherePattern().Match(remaining);
        if (whereMatch.Success)
        {
            sourceFilters = ParseSourceFilters(whereMatch.Groups["where"].Value);
            remaining = remaining[..whereMatch.Index].TrimEnd();
        }

        var conditionMatch = JoinConditionPattern().Match(remaining);
        if (!conditionMatch.Success)
        {
            throw new FormatException("JOIN ON clause is invalid. Expected format: left.Id = right.Id");
        }

        return new ParsedQueryTail(
            conditionMatch.Groups["leftRef"].Value,
            conditionMatch.Groups["rightRef"].Value,
            sourceFilters,
            orderByColumns,
            limit);
    }

    private static SourceFilter[] ParseSourceFilters(string whereSegment) =>
        SplitAndConditions(whereSegment)
            .Select(ParseSourceFilter)
            .ToArray();

    private static IEnumerable<string> SplitAndConditions(string whereSegment)
    {
        var startIndex = 0;
        var inString = false;

        for (var index = 0; index < whereSegment.Length; index++)
        {
            if (whereSegment[index] == '\'')
            {
                if (inString && index + 1 < whereSegment.Length && whereSegment[index + 1] == '\'')
                {
                    index++;
                    continue;
                }

                inString = !inString;
                continue;
            }

            if (!inString && IsAndBoundary(whereSegment, index))
            {
                yield return whereSegment[startIndex..index].Trim();
                index += "AND".Length - 1;
                startIndex = index + 1;
            }
        }

        yield return whereSegment[startIndex..].Trim();
    }

    private static bool IsAndBoundary(string value, int index)
    {
        if (index + "AND".Length > value.Length ||
            !value.AsSpan(index, "AND".Length).Equals("AND", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var hasLeftBoundary = index == 0 || char.IsWhiteSpace(value[index - 1]);
        var rightIndex = index + "AND".Length;
        var hasRightBoundary = rightIndex == value.Length || char.IsWhiteSpace(value[rightIndex]);
        return hasLeftBoundary && hasRightBoundary;
    }

    private static SourceFilter ParseSourceFilter(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new FormatException("WHERE clause contains an empty filter expression.");
        }

        var nullMatch = NullFilterPattern().Match(expression);
        if (nullMatch.Success)
        {
            var reference = FieldReferenceParser.Parse(nullMatch.Groups["ref"].Value, allowWildcard: false);
            var filterOperator = nullMatch.Groups["not"].Success
                ? SourceFilterOperator.IsNotNull
                : SourceFilterOperator.IsNull;

            return new SourceFilter(reference.SourceAlias, reference.SourceField, filterOperator);
        }

        var inMatch = InFilterPattern().Match(expression);
        if (inMatch.Success)
        {
            var reference = FieldReferenceParser.Parse(inMatch.Groups["ref"].Value, allowWildcard: false);
            var values = ParseFilterValues(inMatch.Groups["values"].Value);
            return new SourceFilter(reference.SourceAlias, reference.SourceField, SourceFilterOperator.In, Values: values);
        }

        var containsMatch = ContainsFilterPattern().Match(expression);
        if (containsMatch.Success)
        {
            var reference = FieldReferenceParser.Parse(containsMatch.Groups["ref"].Value, allowWildcard: false);
            return new SourceFilter(
                reference.SourceAlias,
                reference.SourceField,
                SourceFilterOperator.Contains,
                ParseFilterValue(containsMatch.Groups["value"].Value));
        }

        var comparisonMatch = ComparisonFilterPattern().Match(expression);
        if (!comparisonMatch.Success)
        {
            throw new FormatException($"WHERE expression '{expression}' is invalid. Use alias.Field = 'value', !=, <>, >, >=, <, <=, IN (...), CONTAINS 'value', IS NULL, or IS NOT NULL.");
        }

        var comparisonReference = FieldReferenceParser.Parse(comparisonMatch.Groups["ref"].Value, allowWildcard: false);
        var comparisonOperator = ParseFilterOperator(comparisonMatch.Groups["operator"].Value);

        return new SourceFilter(
            comparisonReference.SourceAlias,
            comparisonReference.SourceField,
            comparisonOperator,
            ParseFilterValue(comparisonMatch.Groups["value"].Value));
    }

    private static SourceFilterOperator ParseFilterOperator(string rawOperator) => rawOperator switch
    {
        "=" => SourceFilterOperator.Equals,
        "!=" or "<>" => SourceFilterOperator.NotEquals,
        ">" => SourceFilterOperator.GreaterThan,
        ">=" => SourceFilterOperator.GreaterThanOrEqual,
        "<" => SourceFilterOperator.LessThan,
        "<=" => SourceFilterOperator.LessThanOrEqual,
        _ => throw new FormatException($"WHERE operator '{rawOperator}' is not supported."),
    };

    private static string ParseFilterValue(string rawValue)
    {
        var trimmedValue = rawValue.Trim();
        if (trimmedValue.Length >= 2 && trimmedValue[0] == '\'' && trimmedValue[^1] == '\'')
        {
            return trimmedValue[1..^1].Replace("''", "'", StringComparison.Ordinal);
        }

        return trimmedValue;
    }

    private static string[] ParseFilterValues(string rawValues) =>
        SelectClauseTokenizer.Split(rawValues)
            .Select(ParseFilterValue)
            .ToArray();

    private static OrderByColumn[] ParseOrderByColumns(string orderBySegment) =>
        SelectClauseTokenizer.Split(orderBySegment)
            .Select(ParseOrderByColumn)
            .ToArray();

    private static OrderByColumn ParseOrderByColumn(string expression)
    {
        var match = OrderByColumnPattern().Match(expression.Trim());
        if (!match.Success)
        {
            throw new FormatException($"ORDER BY expression '{expression}' is invalid. Use OutputColumn ASC or OutputColumn DESC.");
        }

        var direction = match.Groups["direction"].Success &&
            string.Equals(match.Groups["direction"].Value, "DESC", StringComparison.OrdinalIgnoreCase)
            ? OrderByDirection.Descending
            : OrderByDirection.Ascending;

        return new OrderByColumn(FieldReferenceParser.UnwrapIdentifier(match.Groups["field"].Value), direction);
    }

    private static int ParseLimit(string rawLimit)
    {
        if (!int.TryParse(rawLimit, CultureInfo.InvariantCulture, out var limit) || limit < 0)
        {
            throw new FormatException($"Limit '{rawLimit}' is invalid. Use zero or a positive integer.");
        }

        return limit;
    }

    [GeneratedRegex(
        "^SELECT\\s+(?<select>.+?)\\s+FROM\\s+(?<leftAlias>[A-Za-z_][A-Za-z0-9_]*)\\s+(?<joinType>INNER|LEFT|RIGHT|FULL)\\s+JOIN\\s+(?<rightAlias>[A-Za-z_][A-Za-z0-9_]*)\\s+ON\\s+(?<tail>.+?)$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex QueryPattern();

    [GeneratedRegex("^TOP\\s+(?:\\(\\s*)?(?<limit>\\d+)(?:\\s*\\))?\\s+", RegexOptions.IgnoreCase)]
    private static partial Regex TopPattern();

    [GeneratedRegex("\\s+LIMIT\\s+(?<limit>\\d+)\\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex LimitPattern();

    [GeneratedRegex("\\s+ORDER\\s+BY\\s+(?<orderBy>.+?)\\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex OrderByPattern();

    [GeneratedRegex("\\s+WHERE\\s+(?<where>.+?)\\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex WherePattern();

    [GeneratedRegex("^(?<leftRef>.+?)\\s*=\\s*(?<rightRef>.+?)$", RegexOptions.Singleline)]
    private static partial Regex JoinConditionPattern();

    [GeneratedRegex("^(?<field>\\[[^\\]]+\\]|[A-Za-z_][A-Za-z0-9_]*)(?:\\s+(?<direction>ASC|DESC))?$", RegexOptions.IgnoreCase)]
    private static partial Regex OrderByColumnPattern();

    [GeneratedRegex("^(?<ref>[A-Za-z_][A-Za-z0-9_]*\\.(?:\\[[^\\]]+\\]|[A-Za-z_][A-Za-z0-9_]*))\\s+IS\\s+(?<not>NOT\\s+)?NULL$", RegexOptions.IgnoreCase)]
    private static partial Regex NullFilterPattern();

    [GeneratedRegex("^(?<ref>[A-Za-z_][A-Za-z0-9_]*\\.(?:\\[[^\\]]+\\]|[A-Za-z_][A-Za-z0-9_]*))\\s+IN\\s*\\((?<values>.+)\\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex InFilterPattern();

    [GeneratedRegex("^(?<ref>[A-Za-z_][A-Za-z0-9_]*\\.(?:\\[[^\\]]+\\]|[A-Za-z_][A-Za-z0-9_]*))\\s+CONTAINS\\s+(?<value>'(?:''|[^'])*'|[^\\s]+)$", RegexOptions.IgnoreCase)]
    private static partial Regex ContainsFilterPattern();

    [GeneratedRegex("^(?<ref>[A-Za-z_][A-Za-z0-9_]*\\.(?:\\[[^\\]]+\\]|[A-Za-z_][A-Za-z0-9_]*))\\s*(?<operator>=|!=|<>|>=|>|<=|<)\\s*(?<value>'(?:''|[^'])*'|[^\\s]+)$", RegexOptions.IgnoreCase)]
    private static partial Regex ComparisonFilterPattern();

    private sealed record ParsedSelectSegment(string SelectSegment, bool IsDistinct, int? Limit);

    private sealed record ParsedQueryTail(
        string LeftReference,
        string RightReference,
        IReadOnlyList<SourceFilter> SourceFilters,
        IReadOnlyList<OrderByColumn> OrderByColumns,
        int? Limit);
}
