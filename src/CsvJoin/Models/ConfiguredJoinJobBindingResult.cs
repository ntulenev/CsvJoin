namespace CsvJoin.Models;

/// <summary>
/// Represents the result of binding application settings to a configured join job.
/// </summary>
internal sealed class ConfiguredJoinJobBindingResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfiguredJoinJobBindingResult"/> class.
    /// </summary>
    /// <param name="job">The configured join job when binding succeeded.</param>
    /// <param name="errors">The binding errors.</param>
    public ConfiguredJoinJobBindingResult(ConfiguredJoinJob? job, IReadOnlyList<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        Job = job;
        Errors = errors;
    }

    /// <summary>
    /// Gets the configured join job when binding succeeded.
    /// </summary>
    public ConfiguredJoinJob? Job { get; }

    /// <summary>
    /// Gets the binding errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Gets a value indicating whether binding succeeded.
    /// </summary>
    public bool IsSuccess => Job is not null && Errors.Count == 0;
}
