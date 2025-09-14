using Books.Core.Models.DTO;

namespace Books.Core.Interfaces;

public interface IBookImportService
{
    Task<ImportResult> ImportFileAsync(string filePath, CancellationToken ct = default);
}
