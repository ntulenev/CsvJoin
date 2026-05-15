namespace CsvJoin.Models;

/// <summary>
/// Represents an output column used to sort the join result.
/// </summary>
/// <param name="OutputField">The output field name to sort by.</param>
/// <param name="Direction">The sort direction.</param>
internal sealed record OrderByColumn(string OutputField, OrderByDirection Direction);
