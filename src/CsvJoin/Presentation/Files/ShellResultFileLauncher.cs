using System.Diagnostics;
using CsvJoin.Abstractions.Presentation;

namespace CsvJoin.Presentation.Files;

internal sealed class ShellResultFileLauncher : IResultFileLauncher
{
    public bool TryOpen(string filePath, out string? errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true,
            });

            errorMessage = null;
            return true;
        }
        #pragma warning disable CA1031
        catch (Exception exception)
        {
            errorMessage = exception.Message;
            return false;
        }
        #pragma warning restore CA1031
    }
}
