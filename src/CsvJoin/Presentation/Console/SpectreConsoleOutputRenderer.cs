using Spectre.Console;

using CsvJoin.Abstractions.Presentation;
using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Presentation.Console;

internal sealed class SpectreConsoleOutputRenderer : IConsoleOutputRenderer
{
    public void RenderHeader(AppSettings settings, CsvJoinQuery query)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(query);

        AnsiConsole.Write(new Rule("[bold deepskyblue1]CSV Join[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.MarkupLine($"[grey]Left source:[/] [silver]{Markup.Escape(settings.Sources[query.LeftAlias].FilePath)}[/]");
        AnsiConsole.MarkupLine($"[grey]Right source:[/] [silver]{Markup.Escape(settings.Sources[query.RightAlias].FilePath)}[/]");
        AnsiConsole.MarkupLine(
            $"[grey]Join:[/] [silver]{Markup.Escape(query.JoinType.ToString().ToUpperInvariant())} on {Markup.Escape(query.LeftAlias)}.{Markup.Escape(query.LeftJoinField)} = {Markup.Escape(query.RightAlias)}.{Markup.Escape(query.RightJoinField)}[/]");
        AnsiConsole.MarkupLine($"[grey]Output directory:[/] [silver]{Markup.Escape(settings.Output.ResultsDirectory)}[/]");
        AnsiConsole.WriteLine();
    }

    public void RenderResult(CsvJoinResult result, int consoleMaxRows)
    {
        ArgumentNullException.ThrowIfNull(result);

        var rowsToRender = consoleMaxRows == 0
            ? result.Rows
            : result.Rows.Take(consoleMaxRows).ToArray();

        AnsiConsole.MarkupLine($"[grey]Result rows:[/] [silver]{result.Rows.Count}[/]");
        AnsiConsole.MarkupLine($"[grey]Result columns:[/] [silver]{result.Headers.Count}[/]");

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Expand();

        foreach (var header in result.Headers)
        {
            _ = table.AddColumn(new TableColumn($"[bold]{Markup.Escape(header)}[/]"));
        }

        foreach (var row in rowsToRender)
        {
            _ = table.AddRow(row.Select(FormatCell).ToArray());
        }

        if (result.Headers.Count > 0)
        {
            AnsiConsole.Write(table);
        }

        if (consoleMaxRows > 0 && result.Rows.Count > consoleMaxRows)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(
                $"[yellow]Console output was truncated to {consoleMaxRows} rows.[/] Full result is saved to file.");
        }

        if (result.Rows.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Join completed with no matching rows.[/]");
        }

        AnsiConsole.WriteLine();
    }

    public void PrintFileSaved(JoinOutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        AnsiConsole.MarkupLine($"[green]Result file saved:[/] [silver]{Markup.Escape(outputFile.FilePath)}[/]");
    }

    public void PrintFileOpened(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        AnsiConsole.MarkupLine($"[grey]Opened result file:[/] [silver]{Markup.Escape(filePath)}[/]");
    }

    public void PrintFileOpenWarning(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        AnsiConsole.MarkupLine($"[yellow]Could not open result file automatically:[/] [silver]{Markup.Escape(message)}[/]");
    }

    private static string FormatCell(string? value) => value is null ? "[grey](null)[/]" : Markup.Escape(value);
}
