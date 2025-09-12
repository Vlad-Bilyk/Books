namespace Books.Core.Interfaces;

public interface IConsoleWrapper
{
    void WriteLine(string message);
    string? ReadLine();
}
