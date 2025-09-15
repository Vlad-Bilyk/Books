using Books.Core.Models.DTO;

namespace Books.Core.Interfaces;

public interface IBookCsvWriter
{
    Task<string> WriteAsync(IEnumerable<BookCsvRow> foundBooks, string? outputDir = null, CancellationToken ct = default);
}
