using System.Text;

namespace CsvJoin.Tests.Csv;

internal sealed class TempCsvFile(string path) : IDisposable
{
    public string Path { get; } = path;

    public static TempCsvFile Create(string content)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");
        System.IO.File.WriteAllText(path, content);
        return new TempCsvFile(path);
    }

    public static TempCsvFile Create(string content, Encoding encoding)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");
        System.IO.File.WriteAllText(path, content, encoding);
        return new TempCsvFile(path);
    }

    public void Dispose()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }
    }
}
