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
    /// <param name="configuredJoinJobBinder">The binder used to validate the configured join job.</param>
    public AppSettingsValidator(IConfiguredJoinJobBinder configuredJoinJobBinder)
    {
        ArgumentNullException.ThrowIfNull(configuredJoinJobBinder);
        _configuredJoinJobBinder = configuredJoinJobBinder;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, AppSettings options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var errors = _configuredJoinJobBinder.Validate(options);
        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }

    private readonly IConfiguredJoinJobBinder _configuredJoinJobBinder;
}
