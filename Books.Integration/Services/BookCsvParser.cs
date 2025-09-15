using Books.Core.Interfaces;
using Books.Core.Models.DTO;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Books.Infrastructure.Services;

public class BookCsvParser : IBookParser
{
    private const string _expectedHeaderLine = "Title,Pages,Genre,ReleaseDate,Author,Publisher";
    private static readonly string[] _expectedHeaderColumns = ["Title", "Pages", "Genre", "ReleaseDate", "Author", "Publisher"];

    private readonly ILogger<BookCsvParser> _logger;

    public BookCsvParser(ILogger<BookCsvParser> logger)
    {
        _logger = logger;
    }

    public IEnumerable<BookCsvRow> Parse(IEnumerable<string> rows)
    {
        const int ExpectedColumns = 6;

        var list = rows.ToList() ?? throw new ArgumentException(null, nameof(rows));
        EnsureValidHeader(list.FirstOrDefault());

        foreach (var row in list.Skip(1)) // skip header
        {
            if (!IsValidRow(row))
            {
                continue;
            }

            var bookData = Regex.Split(row.Trim(), ",(?=\\S)");

            if (bookData.Length != ExpectedColumns)
            {
                _logger.LogWarning("Row does not contain exactly 6 fields. Found {FieldCount}. Skipped: {@Row}", bookData.Length, row);
                continue;
            }

            if (!DateTime.TryParseExact(bookData[3].Trim(), "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime releaseDate))
            {
                _logger.LogWarning("Invalid date format in field[3]. Expected 'yyyy-MM-dd'. Value: {Value}. Skipped: {@Row}", bookData[3], row);
                continue;
            }

            if (!int.TryParse(bookData[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int pages))
            {
                _logger.LogWarning("Page count is not a valid integer. Value: {Value}. Skipped: {@Row}", bookData[1], row);
                continue;
            }

            if (pages < 0)
            {
                _logger.LogWarning("Page count is less than 0: Value: {Value}. Skipped: {@Row}", bookData[1], row);
                continue;
            }

            yield return new BookCsvRow()
            {
                Title = bookData[0].Trim(),
                Pages = pages,
                Genre = bookData[2].Trim(),
                ReleaseDate = releaseDate,
                Author = bookData[4].Trim(),
                Publisher = bookData[5].Trim(),
            };
        }
    }

    private bool IsValidRow(string row)
    {
        if (string.IsNullOrWhiteSpace(row))
        {
            _logger.LogWarning("Row is empty or whitespace. Skipped: {@Row}", row);
            return false;
        }

        if (Regex.IsMatch(row, @"\s+,"))
        {
            _logger.LogWarning("Invalid CSV format: space before comma. Skipped: {@Row}", row);
            return false;
        }

        return true;
    }

    private static void EnsureValidHeader(string? headerLine)
    {
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            throw new InvalidDataException("Header line is empty or whitespace.");
        }

        if (string.Equals(headerLine.Trim(), _expectedHeaderLine, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var headers = headerLine.Split(',');
        if (headers.Length != _expectedHeaderColumns.Length ||
            !headers.SequenceEqual(_expectedHeaderColumns, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Invalid CSV header. Expected: '{_expectedHeaderLine}'. Found: '{headerLine}'.");
        }
    }
}
