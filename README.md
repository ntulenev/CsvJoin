# CsvJoin
<img src="Logo.png" alt="App logo" width="250">

Console utility that reads two CSV files from `appsettings.json`, applies a SQL-like join from configuration, prints the result to the console, saves the full result to a CSV file, and can open that file after completion.

![CsvJoin Screenshot](./Screenshot.png)

## Structure

- `src/CsvJoin` - application project
- `Abstractions / Application / Configuration / Csv / Presentation / Models` - separated responsibilities
- `src/CsvJoin.Tests` - unit tests for parser, join engine, and configuration validation

## Configuration

Main configuration lives in [src/CsvJoin/appsettings.json](src/CsvJoin/appsettings.json).

Example:

```json
{
  "Sources": {
    "left": {
      "FilePath": "sample-data\\left.csv",
      "Delimiter": ",",
      "Encoding": "utf-8",
      "TrimFields": true,
      "NullValues": [ "", "N/A" ],
      "Quote": "\"",
      "IgnoreBlankLines": true
    },
    "right": {
      "FilePath": "sample-data\\right.csv",
      "Delimiter": ",",
      "Encoding": "utf-8",
      "TrimFields": true,
      "NullValues": [ "", "N/A" ],
      "Quote": "\"",
      "IgnoreBlankLines": true
    }
  },
  "ColumnTypes": {
    "right.Score": "number"
  },
  "Query": "SELECT DISTINCT left.Id, left.Name, COALESCE(right.Status, 'Unknown') AS TargetStatus, right.Score AS Score FROM left LEFT JOIN right ON left.Id = right.Id WHERE left.IncludeInReport = 'yes' AND right.Publish = 'yes' AND right.Score >= 90 ORDER BY Score DESC, Name ASC LIMIT 100",
  "JoinKeys": {
    "TrimWhitespace": true,
    "IgnoreCase": true
  },
  "Output": {
    "ResultsDirectory": "results",
    "Delimiter": ",",
    "ConsoleMaxRows": 50,
    "OpenResultAfterBuild": true
  }
}
```

## DSL

Supported query format:

```text
SELECT left.Id, COALESCE(right.[Full Name], 'Unknown') AS Name
FROM left INNER|LEFT|RIGHT|FULL JOIN right
ON left.Id = right.ExternalId
WHERE left.Country IS NOT NULL AND right.Status IN ('Active', 'Blocked') AND right.Score >= 90
ORDER BY Score DESC
LIMIT 100
```

Supported features:

- `DISTINCT`
- `INNER`, `LEFT`, `RIGHT`, `FULL`
- field aliases via `AS`
- per-column fallback values via `COALESCE(alias.Field, 'default')`
- headers with spaces via brackets: `right.[Full Name]`
- wildcard selection: `left.*`, `right.*`
- source-row filtering before join via `WHERE alias.Field = 'value'`, `!=`, `<>`, `>`, `>=`, `<`, `<=`, `IN (...)`, `CONTAINS`, `IS NULL`, `IS NOT NULL`, joined with `AND`
- sorting by output columns via `ORDER BY Column ASC|DESC`
- result limits via `LIMIT n` or `TOP n`
- typed source columns via `ColumnTypes`, currently `text`, `number`, and `date`; typed columns affect range filters and sorting

## Run

```powershell
dotnet run --project .\src\CsvJoin
```

## Output

- result is rendered to the console
- full CSV is written to `results\<file1>_<file2>_<timestamp>.csv`
- when `OpenResultAfterBuild` is `true`, the result file is opened via shell

## CSV input options

Each source can configure `Encoding`, `TrimFields`, `NullValues`, `Quote`, and `IgnoreBlankLines`.

`JoinKeys` controls matching only: `TrimWhitespace` removes leading/trailing spaces from join keys, and `IgnoreCase` makes key comparison case-insensitive.

`ColumnTypes` is keyed as `alias.Field`, for example `"right.Score": "number"`. Fields not listed there are treated as `text`.

## Tests

```powershell
dotnet test .\src\CsvJoin.slnx
```
