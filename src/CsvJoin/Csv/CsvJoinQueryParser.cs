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

        var conditionMatch = JoinConditionPattern().Match(remaining);
        if (!conditionMatch.Success)
        {
            throw new FormatException("JOIN ON clause is invalid. Expected format: left.Id = right.Id");
        }

        return new ParsedQueryTail(
            conditionMatch.Groups["leftRef"].Value,
            conditionMatch.Groups["rightRef"].Value,
            orderByColumns,
            limit);
    }

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

    [GeneratedRegex("^(?<leftRef>.+?)\\s*=\\s*(?<rightRef>.+?)$", RegexOptions.Singleline)]
    private static partial Regex JoinConditionPattern();

    [GeneratedRegex("^(?<field>\\[[^\\]]+\\]|[A-Za-z_][A-Za-z0-9_]*)(?:\\s+(?<direction>ASC|DESC))?$", RegexOptions.IgnoreCase)]
    private static partial Regex OrderByColumnPattern();

    private sealed record ParsedSelectSegment(string SelectSegment, bool IsDistinct, int? Limit);

    private sealed record ParsedQueryTail(
        string LeftReference,
        string RightReference,
        IReadOnlyList<OrderByColumn> OrderByColumns,
        int? Limit);
}
