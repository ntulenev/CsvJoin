using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Abstractions.Presentation;

internal interface IConsoleOutputRenderer
{
    void RenderHeader(AppSettings settings, CsvJoinQuery query);

    void RenderResult(CsvJoinResult result, int consoleMaxRows);

    void PrintFileSaved(JoinOutputFile outputFile);

    void PrintFileOpened(string filePath);

    void PrintFileOpenWarning(string message);
}
