using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Abstractions.Application;

/// <summary>
/// Binds application settings to an executable join job, including environment-dependent checks.
/// </summary>
internal interface IConfiguredJoinJobBinder
{
    /// <summary>
     /// Binds application settings to an executable join job.
     /// </summary>
     /// <param name="settings">The application settings to bind.</param>
    /// <returns>The binding result.</returns>
    ConfiguredJoinJobBindingResult Bind(AppSettings settings);
}
