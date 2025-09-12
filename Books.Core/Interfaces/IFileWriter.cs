namespace Books.Core.Interfaces;

public interface IFileWriter
{
    void WriteLines(string filePath, IEnumerable<string> lines);
}
