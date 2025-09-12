using Books.Core.Models.DTO;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Books.Infrastructure.Services;

public class BookCsvParser
{
    private readonly ILogger<BookCsvParser> _logger;

    public BookCsvParser(ILogger<BookCsvParser> logger)
    {
        _logger = logger;
    }

    public IEnumerable<ParsedBookRow> Parse(IEnumerable<string> rows)
    {
        foreach (var row in rows.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(row))
            {
                _logger.LogWarning("Row is empty or whitespace. Skipped: {@Row}", row);
                continue;
            }

            var bookData = Regex.Split(row, ",(?=\\S)");

            if (bookData.Length != 6)
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

            yield return new ParsedBookRow()
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
}
