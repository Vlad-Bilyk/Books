namespace Books.Core.Interfaces;

/// <summary>
/// Console I/O abstraction.
/// </summary>
public interface IConsoleWrapper
{
    /// <summary>
    /// Writes a line to the console.
    /// </summary>
    /// <param name="message">Text to write.</param>
    void WriteLine(string message);

    /// <summary>
    /// Reads a line from the console.
    /// </summary>
    /// <returns>User input or null on EOF.</returns>
    string? ReadLine();
}
