namespace Books.Core.Interfaces;

/// <summary>
/// Reads file content.
/// </summary>
public interface IFileReader
{
    /// <summary>
    /// Reads all lines from a file.
    /// </summary>
    /// <param name="filePath">File path.</param>
    /// <returns>File lines.</returns>
    IEnumerable<string> ReadLines(string filePath);

    /// <summary>
    /// Reads the entire file as a single string.
    /// </summary>
    /// <param name="filePath">File path.</param>
    /// <returns>File content.</returns>
    string ReadAllText(string filePath);
}
