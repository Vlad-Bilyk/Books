namespace Books.App.IO;

public interface IFileReader
{
    IEnumerable<string> ReadLines(string filePath);
}
