using CsvJoin.Models;

namespace CsvJoin.Abstractions.Csv;

internal interface ICsvJoinQueryParser
{
    CsvJoinQuery Parse(string queryText);
}
