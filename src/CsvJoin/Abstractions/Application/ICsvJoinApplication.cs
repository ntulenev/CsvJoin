namespace CsvJoin.Abstractions.Application;

/// <summary>
/// Runs the CSV join use case from the configured application settings.
/// </summary>
internal interface ICsvJoinApplication
{
    /// <summary>
    /// Executes the configured CSV join pipeline.
    /// </summary>
    /// <param name="cancellationToken">Signals that the operation should be canceled.</param>
    /// <returns>The process exit code for the execution result.</returns>
    Task<int> RunAsync(CancellationToken cancellationToken);
}
