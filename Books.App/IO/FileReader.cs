using Books.Core.Interfaces;

namespace Books.App.IO;

public class FileReader : IFileReader
{
    public IEnumerable<string> ReadLines(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Path is null or empty", nameof(filePath));
        }

        return ReadLinesIterator(filePath);
    }

    public string ReadAllText(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Path is null or empty", nameof(filePath));
        }

        return File.ReadAllText(filePath);
    }

    private static IEnumerable<string> ReadLinesIterator(string filePath)
    {
        using var reader = new StreamReader(filePath);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            yield return line;
        }
    }
}
