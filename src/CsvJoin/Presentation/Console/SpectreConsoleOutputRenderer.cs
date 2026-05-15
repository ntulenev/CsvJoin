using System.Text;

using Spectre.Console;

using CsvJoin.Abstractions.Presentation;
using CsvJoin.Models;

namespace CsvJoin.Presentation.Console;

/// <summary>
/// Renders execution details and results with Spectre.Console.
/// </summary>
internal sealed class SpectreConsoleOutputRenderer : IConsoleOutputRenderer
{
    /// <inheritdoc />
    public void RenderHeader(ConfiguredJoinJob job)
    {
        ArgumentNullException.ThrowIfNull(job);

        AnsiConsole.Write(new Rule("[bold deepskyblue1]CSV Join[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.MarkupLine($"[grey]Left source:[/] [silver]{Markup.Escape(job.LeftSource.FilePath)}[/]");
        AnsiConsole.MarkupLine($"[grey]Right source:[/] [silver]{Markup.Escape(job.RightSource.FilePath)}[/]");
        AnsiConsole.MarkupLine(
            $"[grey]Join:[/] [silver]{Markup.Escape(job.Query.JoinType.ToString().ToUpperInvariant())} on {Markup.Escape(job.Query.LeftAlias)}.{Markup.Escape(job.Query.LeftJoinField)} = {Markup.Escape(job.Query.RightAlias)}.{Markup.Escape(job.Query.RightJoinField)}[/]");
        AnsiConsole.MarkupLine($"[grey]Query:[/] {FormatQuery(job.QueryText)}");
        AnsiConsole.MarkupLine($"[grey]Output directory:[/] [silver]{Markup.Escape(job.Output.ResultsDirectory)}[/]");
        AnsiConsole.WriteLine();
    }

    /// <inheritdoc />
    public void RenderResult(CsvJoinResult result, int consoleMaxRows)
    {
        ArgumentNullException.ThrowIfNull(result);

        var rowsToRender = consoleMaxRows == 0
            ? result.Rows
            : result.Rows.Take(consoleMaxRows).ToArray();

        AnsiConsole.MarkupLine($"[grey]Result rows:[/] [silver]{result.Rows.Count}[/]");
        AnsiConsole.MarkupLine($"[grey]Result columns:[/] [silver]{result.Headers.Count}[/]");
        RenderDiagnostics(result.Diagnostics);

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

    /// <inheritdoc />
    public void PrintFileSaved(JoinOutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        AnsiConsole.MarkupLine($"[green]Result file saved:[/] [silver]{Markup.Escape(outputFile.FilePath)}[/]");
    }

    /// <inheritdoc />
    public void PrintFileOpened(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        AnsiConsole.MarkupLine($"[grey]Opened result file:[/] [silver]{Markup.Escape(filePath)}[/]");
    }

    /// <inheritdoc />
    public void PrintFileOpenWarning(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        AnsiConsole.MarkupLine($"[yellow]Could not open result file automatically:[/] [silver]{Markup.Escape(message)}[/]");
    }

    private static string FormatCell(string? value) => value is null ? "[grey](null)[/]" : Markup.Escape(value);

    private static void RenderDiagnostics(JoinDiagnostics? diagnostics)
    {
        if (diagnostics is null)
        {
            return;
        }

        AnsiConsole.MarkupLine(
            $"[grey]Left rows:[/] [silver]{diagnostics.LeftSourceRows}[/] [grey]->[/] [silver]{diagnostics.LeftRowsAfterFilters}[/] [grey]after filters[/]");
        AnsiConsole.MarkupLine(
            $"[grey]Right rows:[/] [silver]{diagnostics.RightSourceRows}[/] [grey]->[/] [silver]{diagnostics.RightRowsAfterFilters}[/] [grey]after filters[/]");
        AnsiConsole.MarkupLine($"[grey]Matched pairs:[/] [silver]{diagnostics.MatchedRowPairs}[/]");
        AnsiConsole.MarkupLine(
            $"[grey]Unmatched left/right:[/] [silver]{diagnostics.UnmatchedLeftRows}[/][grey]/[/][silver]{diagnostics.UnmatchedRightRows}[/]");
        AnsiConsole.MarkupLine(
            $"[grey]Duplicate join keys left/right:[/] [silver]{diagnostics.DuplicateLeftJoinKeys}[/][grey]/[/][silver]{diagnostics.DuplicateRightJoinKeys}[/]");
        AnsiConsole.MarkupLine(
            $"[grey]Rows before DISTINCT/ORDER/LIMIT:[/] [silver]{diagnostics.ProjectedRowsBeforeResultOptions}[/]");
    }

    private static string FormatQuery(string queryText)
    {
        var builder = new StringBuilder(queryText.Length);

        for (var index = 0; index < queryText.Length;)
        {
            var current = queryText[index];
            if (char.IsWhiteSpace(current))
            {
                builder.Append(current);
                index++;
                continue;
            }

            if (current == '\'')
            {
                index = AppendQuotedString(queryText, index, builder);
                continue;
            }

            if (char.IsLetter(current) || current == '_')
            {
                index = AppendWord(queryText, index, builder);
                continue;
            }

            if (char.IsDigit(current))
            {
                index = AppendNumber(queryText, index, builder);
                continue;
            }

            builder.Append("[grey]").Append(Markup.Escape(current.ToString())).Append("[/]");
            index++;
        }

        return builder.ToString();
    }

    private static int AppendQuotedString(string queryText, int startIndex, StringBuilder builder)
    {
        var endIndex = startIndex + 1;
        while (endIndex < queryText.Length)
        {
            if (queryText[endIndex] == '\'')
            {
                if (endIndex + 1 < queryText.Length && queryText[endIndex + 1] == '\'')
                {
                    endIndex += 2;
                    continue;
                }

                endIndex++;
                break;
            }

            endIndex++;
        }

        builder
            .Append("[darkseagreen3]")
            .Append(Markup.Escape(queryText[startIndex..endIndex]))
            .Append("[/]");

        return endIndex;
    }

    private static int AppendWord(string queryText, int startIndex, StringBuilder builder)
    {
        var endIndex = startIndex + 1;
        while (endIndex < queryText.Length && (char.IsLetterOrDigit(queryText[endIndex]) || queryText[endIndex] == '_'))
        {
            endIndex++;
        }

        var word = queryText[startIndex..endIndex];
        if (QueryKeywords.Contains(word))
        {
            builder.Append("[bold deepskyblue1]").Append(Markup.Escape(word)).Append("[/]");
        }
        else if (QueryFunctions.Contains(word))
        {
            builder.Append("[mediumpurple1]").Append(Markup.Escape(word)).Append("[/]");
        }
        else
        {
            builder.Append("[silver]").Append(Markup.Escape(word)).Append("[/]");
        }

        return endIndex;
    }

    private static int AppendNumber(string queryText, int startIndex, StringBuilder builder)
    {
        var endIndex = startIndex + 1;
        while (endIndex < queryText.Length && char.IsDigit(queryText[endIndex]))
        {
            endIndex++;
        }

        builder
            .Append("[orange1]")
            .Append(Markup.Escape(queryText[startIndex..endIndex]))
            .Append("[/]");

        return endIndex;
    }

    private static readonly HashSet<string> QueryKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "AS",
        "ASC",
        "AND",
        "BY",
        "DESC",
        "DISTINCT",
        "FROM",
        "FULL",
        "INNER",
        "JOIN",
        "LEFT",
        "LIMIT",
        "IS",
        "NOT",
        "NULL",
        "ON",
        "ORDER",
        "RIGHT",
        "SELECT",
        "TOP",
    };

    private static readonly HashSet<string> QueryFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "COALESCE",
    };
}
