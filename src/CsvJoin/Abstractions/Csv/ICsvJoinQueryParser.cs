using CsvJoin.Models;

namespace CsvJoin.Abstractions.Csv;

/// <summary>
/// Parses SQL-like CSV join queries.
/// </summary>
internal interface ICsvJoinQueryParser
{
    /// <summary>
    /// Parses a join query into a domain query model.
    /// </summary>
    /// <param name="queryText">The query text to parse.</param>
    /// <returns>The parsed join query.</returns>
    CsvJoinQuery Parse(string queryText);
}
