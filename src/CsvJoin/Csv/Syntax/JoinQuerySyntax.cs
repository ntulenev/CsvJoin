using CsvJoin.Models;

namespace CsvJoin.Csv.Syntax;

/// <summary>
/// Represents a parsed DSL query before it is mapped to the domain model.
/// </summary>
/// <param name="Projections">The parsed select projections.</param>
/// <param name="LeftAlias">The declared left source alias.</param>
/// <param name="RightAlias">The declared right source alias.</param>
/// <param name="JoinType">The join type to execute.</param>
/// <param name="JoinCondition">The join condition.</param>
internal sealed record JoinQuerySyntax(
    IReadOnlyList<SelectProjectionSyntax> Projections,
    string LeftAlias,
    string RightAlias,
    JoinType JoinType,
    JoinConditionSyntax JoinCondition)
{
    /// <summary>
    /// Converts the syntax tree to the domain query model.
    /// </summary>
    /// <returns>The domain query model.</returns>
    public CsvJoinQuery ToDomainQuery()
    {
        if (Projections.Count == 0)
        {
            throw new FormatException("SELECT clause must contain at least one column.");
        }

        var (leftJoinField, rightJoinField) = JoinCondition.ResolveJoinFields(LeftAlias, RightAlias);
        var selectColumns = Projections.Select(static projection => projection.ToSelectColumn()).ToArray();
        return new CsvJoinQuery(LeftAlias, leftJoinField, RightAlias, rightJoinField, JoinType, selectColumns);
    }
}
