using Books.App.IO;
using Books.Core.Interfaces;
using Books.Infrastructure.Data;

namespace Books.App;

public class AppRunner
{
    private readonly IConsoleWrapper _console;
    private readonly IFileReader _fileReader;
    private readonly IBookCsvParser _bookCsvParser;
    private readonly AppDbContext _dbContext;

    public AppRunner(IConsoleWrapper console, IFileReader fileReader, IBookCsvParser bookCsvParser,
        AppDbContext dbContext)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
        _bookCsvParser = bookCsvParser ?? throw new ArgumentNullException(nameof(bookCsvParser));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Run()
    {
        _console.WriteLine("Input file path: ");
        var inPath = _console.ReadLine()?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(inPath))
        {
            _console.WriteLine("Input/output path is empty.");
            return;
        }

        var inFull = Path.GetFullPath(inPath);

        var lines = _fileReader.ReadLines(inFull);
        var books = _bookCsvParser.Parse(lines);
    }
}
