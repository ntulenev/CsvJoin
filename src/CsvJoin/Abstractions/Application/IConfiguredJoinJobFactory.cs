using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Abstractions.Application;

/// <summary>
/// Creates an executable join job from application settings.
/// </summary>
internal interface IConfiguredJoinJobFactory
{
    /// <summary>
    /// Creates a configured join job for runtime execution.
    /// </summary>
    /// <param name="settings">The application settings to translate into a join job.</param>
    /// <returns>A configured join job.</returns>
    ConfiguredJoinJob Create(AppSettings settings);
}
