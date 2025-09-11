namespace Books.App.IO;

public class ConsoleWrapper : IConsoleWrapper
{
    public string? ReadLine()
    {
        return Console.ReadLine();
    }

    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }
}
