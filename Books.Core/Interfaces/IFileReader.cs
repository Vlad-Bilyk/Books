namespace Books.Core.Interfaces;

public interface IFileReader
{
    IEnumerable<string> ReadLines(string filePath);
}
