using Books.Core.Models.DTO;

namespace Books.Core.Interfaces;

public interface IBookCsvParser
{
    IEnumerable<ParsedBookRow> Parse(IEnumerable<string> rows);
}
