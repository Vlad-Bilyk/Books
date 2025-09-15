using Books.Core.Interfaces;
using Books.Core.Models.DTO;
using Books.Core.Models.Entities;

namespace Books.App;

public class AppRunner
{
    private readonly IConsoleWrapper _console;
    private readonly IBookImportService _bookImportService;
    private readonly IBookSearchService _bookSearchService;
    private readonly IFilterReader _filterReader;
    private readonly IBookCsvWriter _csvWriter;

    public AppRunner(IConsoleWrapper console, IBookImportService bookImportService,
        IBookSearchService bookSearchService, IFilterReader filterReader, IBookCsvWriter csvWriter)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _bookImportService = bookImportService ?? throw new ArgumentNullException(nameof(bookImportService));
        _bookSearchService = bookSearchService ?? throw new ArgumentNullException(nameof(bookSearchService));
        _filterReader = filterReader ?? throw new ArgumentNullException(nameof(filterReader));
        _csvWriter = csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        while (true)
        {
            _console.WriteLine("\n====Books====");
            _console.WriteLine("1. Import books from CSV file");
            _console.WriteLine("2. Search books");
            _console.WriteLine("0. Exit");
            _console.WriteLine("Select an option: ");

            var input = _console.ReadLine()?.Trim();

            if (input == "1")
            {
                await ImportBooksAsync(ct);
            }
            else if (input == "2")
            {
                var foundBooks = await SearchBooks(ct);
                PrintBooks(foundBooks);

                _console.WriteLine("\nExport results to CSV? (y/n): ");
                input = _console.ReadLine()?.Trim();
                input = input?.ToLowerInvariant();

                if (input == "y" || input == "yes")
                {
                    _console.WriteLine("Enter ouput directory (leave blank to use app folder)");
                    input = _console.ReadLine()?.Trim();

                    await ExportBooksAsync(foundBooks, input, ct);
                }
                else if (input == "n" || input == "no")
                {
                    _console.WriteLine("Export cancelled. Returning to main menu.");
                }
                else
                {
                    _console.WriteLine("Invalid option. Returning to main menu.");
                }
            }
            else if (input == "0")
            {
                break;
            }
            else
            {
                _console.WriteLine("Invalid option. Please try again.");
            }
        }
    }

    private async Task ImportBooksAsync(CancellationToken ct = default)
    {
        _console.WriteLine("Input file path: ");
        var inPath = _console.ReadLine()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(inPath))
        {
            _console.WriteLine("Input path is empty.");
            return;
        }
        var inFull = Path.GetFullPath(inPath);

        try
        {
            var result = await _bookImportService.ImportFileAsync(inFull, ct);
            _console.WriteLine($"\nAdded: {result.Added}, Skipped duplicates: {result.SkippedDuplicates}");
        }
        catch (Exception ex)
        {
            _console.WriteLine($"Error importing file: {ex.Message}");
        }
    }

    private async Task<List<Book>> SearchBooks(CancellationToken ct = default)
    {
        _console.WriteLine("Input search filter file path: ");
        var filterPath = _console.ReadLine()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(filterPath))
        {
            _console.WriteLine("Filter path is empty.");
            return [];
        }

        var filterFull = Path.GetFullPath(filterPath);

        try
        {
            var filter = _filterReader.Read(filterFull);
            PrintActiveFilters(filter);
            return await _bookSearchService.SearchBooksAsync(filter, ct);
        }
        catch (Exception ex)
        {
            _console.WriteLine($"Error reading filter: {ex.Message}");
            return [];
        }
    }

    private void PrintBooks(List<Book> books)
    {
        _console.WriteLine("\nSearch results:");
        _console.WriteLine($"Total: {books.Count}");

        foreach (var book in books)
        {
            _console.WriteLine($"- {book.Title}");
        }
    }

    private void PrintActiveFilters(Filter filter)
    {
        _console.WriteLine("\nActive filters:");
        if (!string.IsNullOrEmpty(filter.Title))
        {
            _console.WriteLine($"- Title: {filter.Title}");
        }
        if (!string.IsNullOrEmpty(filter.Genre))
        {
            _console.WriteLine($"- Genre: {filter.Genre}");
        }
        if (!string.IsNullOrEmpty(filter.Author))
        {
            _console.WriteLine($"- Author: {filter.Author}");
        }
        if (!string.IsNullOrEmpty(filter.Publisher))
        {
            _console.WriteLine($"- Publisher: {filter.Publisher}");
        }
        if (filter.MoreThanPages.HasValue)
        {
            _console.WriteLine($"- More than pages: {filter.MoreThanPages}");
        }
        if (filter.LessThanPages.HasValue)
        {
            _console.WriteLine($"- Less than pages: {filter.LessThanPages}");
        }
        if (filter.PublishedAfter.HasValue)
        {
            _console.WriteLine($"- Published after: {filter.PublishedAfter:yyyy-MM-dd}");
        }
        if (filter.PublishedBefore.HasValue)
        {
            _console.WriteLine($"- Published before: {filter.PublishedBefore:yyyy-MM-dd}");
        }
    }

    private async Task ExportBooksAsync(IEnumerable<Book> foundBooks, string? outputDir = null, CancellationToken ct = default)
    {
        var books = foundBooks
            .OrderBy(b => b.Title)
            .Select(b => new BookCsvRow
            {
                Title = b.Title,
                Pages = b.Pages,
                Genre = b.Genre.Name,
                ReleaseDate = b.ReleaseDate,
                Author = b.Author.Name,
                Publisher = b.Publisher.Name
            }
        );

        try
        {
            var path = await _csvWriter.WriteAsync(books, outputDir, ct);
            _console.WriteLine($"Export complete: {path}");
        }
        catch (Exception ex)
        {
            _console.WriteLine($"Error exporting to CSV: {ex.Message}");
        }
    }
}
