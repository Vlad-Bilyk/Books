namespace Books.Core.Interfaces;

public interface IFileReader
{
    IEnumerable<string> ReadLines(string filePath);
    string ReadAllText(string filePath);
}
