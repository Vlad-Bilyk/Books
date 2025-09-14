using Books.Core.Models.DTO;

namespace Books.Core.Interfaces;

public interface IBookParser
{
    public IEnumerable<ParsedBookRow> Parse(IEnumerable<string> rows);
}
