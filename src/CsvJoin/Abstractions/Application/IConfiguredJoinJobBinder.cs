using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Abstractions.Application;

/// <summary>
/// Binds validated application settings to an executable join job.
/// </summary>
internal interface IConfiguredJoinJobBinder
{
    /// <summary>
    /// Binds application settings to an executable join job.
    /// </summary>
    /// <param name="settings">The application settings to bind.</param>
    /// <returns>The configured join job.</returns>
    ConfiguredJoinJob Bind(AppSettings settings);

    /// <summary>
    /// Validates that application settings can be bound to an executable join job.
    /// </summary>
    /// <param name="settings">The application settings to validate.</param>
    /// <returns>The validation errors, or an empty collection when binding is valid.</returns>
    IReadOnlyList<string> Validate(AppSettings settings);
}
