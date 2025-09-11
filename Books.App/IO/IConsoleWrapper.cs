namespace Books.App.IO;

public interface IConsoleWrapper
{
    void WriteLine(string message);
    string? ReadLine();
}
