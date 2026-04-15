using CsvJoin.Models;

namespace CsvJoin.Abstractions.Csv;

internal interface ICsvJoinProcessor
{
    CsvJoinResult Process(CsvJoinQuery query, CsvDataSet left, CsvDataSet right);
}
