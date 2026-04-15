# CsvJoin

Console utility that reads two CSV files from `appsettings.json`, applies a SQL-like join from configuration, prints the result to the console, saves the full result to a CSV file, and can open that file after completion.

![CsvJoin Screenshot](./Screenshot.png)

## Structure

The project follows the same high-level approach as `NReleaseBuilder`:

- `src/CsvJoin` - application project
- `Abstractions / Application / Configuration / Csv / Presentation / Models` - separated responsibilities
- `src/CsvJoin.Tests` - unit tests for parser, join engine, and configuration validation

## Configuration

Main configuration lives in [src/CsvJoin/appsettings.json](C:\Users\ntyulenev\Documents\Code\CsvJoin\src\CsvJoin\appsettings.json).

Example:

```json
{
  "Sources": {
    "left": {
      "FilePath": "sample-data\\left.csv",
      "Delimiter": ","
    },
    "right": {
      "FilePath": "sample-data\\right.csv",
      "Delimiter": ","
    }
  },
  "Query": "SELECT left.Id, left.Name, COALESCE(right.Status, 'Unknown') AS TargetStatus FROM left LEFT JOIN right ON left.Id = right.Id",
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
```

Supported features:

- `INNER`, `LEFT`, `RIGHT`, `FULL`
- field aliases via `AS`
- per-column fallback values via `COALESCE(alias.Field, 'default')`
- headers with spaces via brackets: `right.[Full Name]`
- wildcard selection: `left.*`, `right.*`

## Run

```powershell
dotnet run --project .\src\CsvJoin
```

## Output

- result is rendered to the console
- full CSV is written to `results\<file1>_<file2>_<timestamp>.csv`
- when `OpenResultAfterBuild` is `true`, the result file is opened via shell

## Tests

```powershell
dotnet test .\src\CsvJoin.slnx
```
