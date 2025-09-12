namespace Books.Core.Interfaces;

public interface IBookImportService
{
    Task ImportFileAsync(string filePath, CancellationToken ct = default);
}
