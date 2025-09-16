using Books.Core.Interfaces;
using Books.Core.Models.DTO;
using Microsoft.Extensions.Logging;

namespace Books.Infrastructure.Services;

public class CsvExportWriter : IBookCsvWriter
{
    private const string _csvFileExtension = ".csv";
    private const string _fileNamePrefix = "books_";
    private const string _headerLine = "Title,Pages,Genre,ReleaseDate,Author,Publisher";

    private readonly ILogger<CsvExportWriter> _logger;
    private readonly TimeProvider _timeProvider;

    public CsvExportWriter(ILogger<CsvExportWriter> logger, TimeProvider timeProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<string> WriteAsync(IEnumerable<BookCsvRow> foundBooks, string? outputDir = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(foundBooks);

        var list = foundBooks.ToList();

        var dir = string.IsNullOrWhiteSpace(outputDir) ? AppContext.BaseDirectory : outputDir!;
        Directory.CreateDirectory(dir);

        var fileName = CreateFileName();
        var fullPath = Path.Combine(dir, fileName);

        _logger.LogInformation("Writing {Count} row(s) to CSV: {FilePath}", list.Count, fullPath);

        await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await using var writer = new StreamWriter(stream);

        await writer.WriteLineAsync(_headerLine);

        foreach (var book in list)
        {
            ct.ThrowIfCancellationRequested();

            await writer.WriteLineAsync($"{book.Title},{book.Pages},{book.Genre},{book.ReleaseDate.ToString("yyyy-MM-dd")},{book.Author},{book.Publisher}");
        }

        await writer.FlushAsync();
        _logger.LogInformation("CSV file saved: {FilePath}", fullPath);

        return fullPath;
    }

    private string CreateFileName()
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        return $"{_fileNamePrefix}{utcNow:yyyyMMdd_HHmmss}Z{_csvFileExtension}";
    }
}
