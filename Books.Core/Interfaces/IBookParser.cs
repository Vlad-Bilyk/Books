using Books.Core.Models.DTO;

namespace Books.Core.Interfaces;

public interface IBookParser
{
    public IEnumerable<BookCsvRow> Parse(IEnumerable<string> rows);
}
