using Microsoft.Extensions.Options;

using CsvJoin.Abstractions.Application;

namespace CsvJoin.Configuration;

/// <summary>
/// Validates application settings before execution starts.
/// </summary>
internal sealed class AppSettingsValidator : IValidateOptions<AppSettings>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingsValidator"/> class.
    /// </summary>
    /// <param name="settingsBinder">The binder used to validate pure configuration rules.</param>
    public AppSettingsValidator(IConfiguredJoinJobSettingsBinder settingsBinder)
    {
        ArgumentNullException.ThrowIfNull(settingsBinder);
        _settingsBinder = settingsBinder;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, AppSettings options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var errors = _settingsBinder.Bind(options).Errors;
        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }

    private readonly IConfiguredJoinJobSettingsBinder _settingsBinder;
}
