using Books.Core.Interfaces;

namespace Books.App;

public class AppRunner
{
    private readonly IConsoleWrapper _console;
    private readonly IBookImportService _bookImportService;

    public AppRunner(IConsoleWrapper console, IBookImportService bookImportService)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _bookImportService = bookImportService;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            _console.WriteLine("\n====Books====");
            _console.WriteLine("1. Import books from CSV file");
            _console.WriteLine("0. Exit");
            _console.WriteLine("Select an option: ");

            var input = _console.ReadLine()?.Trim();

            if (input == "1")
            {
                await ImportBooksAsync();
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

    private async Task ImportBooksAsync()
    {
        _console.WriteLine("Input file path: ");
        var inPath = _console.ReadLine()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(inPath))
        {
            _console.WriteLine("Input/output path is empty.");
            return;
        }
        var inFull = Path.GetFullPath(inPath);
        var result = await _bookImportService.ImportFileAsync(inFull);
        _console.WriteLine($"\nAdded: {result.Added}, Skipped duplicates: {result.SkippedDuplicates}");
    }
}
