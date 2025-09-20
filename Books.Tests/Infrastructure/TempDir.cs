namespace Books.Tests.Infrastructure;

/// <summary>
/// Creates and cleans up a unique temporary directory for file-based tests.
/// </summary>
internal sealed class TempDir : IDisposable
{
    public string Path { get; }

    public TempDir()
    {
        var name = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "books_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(name);
        Path = name;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
