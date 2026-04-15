using CsvJoin.Abstractions.Application;
using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Application;

/// <summary>
/// Binds application settings to executable join jobs.
/// </summary>
internal sealed class ConfiguredJoinJobBinder : IConfiguredJoinJobBinder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfiguredJoinJobBinder"/> class.
    /// </summary>
    /// <param name="settingsBinder">The binder used to validate and translate pure configuration settings.</param>
    public ConfiguredJoinJobBinder(IConfiguredJoinJobSettingsBinder settingsBinder)
    {
        ArgumentNullException.ThrowIfNull(settingsBinder);
        _settingsBinder = settingsBinder;
    }

    /// <inheritdoc />
    public ConfiguredJoinJobBindingResult Bind(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var settingsBindingResult = _settingsBinder.Bind(settings);
        if (!settingsBindingResult.IsSuccess || settingsBindingResult.Job is null)
        {
            return settingsBindingResult;
        }

        var errors = ValidateSourceFiles(settingsBindingResult.Job);
        return errors.Count == 0
            ? settingsBindingResult
            : new ConfiguredJoinJobBindingResult(settingsBindingResult.Job, errors);
    }

    private static List<string> ValidateSourceFiles(ConfiguredJoinJob job)
    {
        var errors = new List<string>();

        ValidateSourceFile(job.LeftSource, errors);
        ValidateSourceFile(job.RightSource, errors);

        return errors;
    }

    private static void ValidateSourceFile(ConfiguredCsvSource source, List<string> errors)
    {
        if (!File.Exists(source.FilePath))
        {
            errors.Add($"CSV file not found for source '{source.Alias}': {source.FilePath}");
        }
    }

    private readonly IConfiguredJoinJobSettingsBinder _settingsBinder;
}
