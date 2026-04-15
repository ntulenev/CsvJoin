using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Abstractions.Application;

/// <summary>
/// Binds application settings to an executable join job using pure configuration validation only.
/// </summary>
internal interface IConfiguredJoinJobSettingsBinder
{
    /// <summary>
    /// Binds application settings to an executable join job using pure configuration rules.
    /// </summary>
    /// <param name="settings">The application settings to bind.</param>
    /// <returns>The binding result.</returns>
    ConfiguredJoinJobBindingResult Bind(AppSettings settings);
}
