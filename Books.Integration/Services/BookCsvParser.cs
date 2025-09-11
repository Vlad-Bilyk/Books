using Books.Core.Interfaces;
using Books.Core.Models.DTO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Books.Infrastructure.Services;

public class BookCsvParser : IBookCsvParser
{
    public IEnumerable<ParsedBookRow> Parse(IEnumerable<string> rows)
    {
        foreach (var row in rows.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(row))
            {
                // TODO: log error
                continue;
            }

            var bookData = Regex.Split(row, ",(?=\\S)");

            if (bookData.Length != 6)
            {
                // TODO: log error
                continue;
            }

            if (!DateTime.TryParseExact(bookData[3].Trim(), "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime releaseDate))
            {
                // TODO: log error
                continue;
            }

            if (!int.TryParse(bookData[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int pages))
            {
                // TODO: log error
                continue;
            }

            if (pages < 0)
            {
                // TODO: log error
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
