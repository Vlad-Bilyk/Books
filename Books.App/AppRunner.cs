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
        _console.WriteLine("Input file path: ");
        var inPath = _console.ReadLine()?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(inPath))
        {
            _console.WriteLine("Input/output path is empty.");
            return;
        }

        var inFull = Path.GetFullPath(inPath);
        await _bookImportService.ImportFileAsync(inFull);
    }
}
