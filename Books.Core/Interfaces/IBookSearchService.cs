using Books.Core.Models.DTO;
using Books.Core.Models.Entities;

namespace Books.Core.Interfaces;

public interface IBookSearchService
{
    Task<List<Models.Entities.Book>> SearchBooksAsync(Filter filter, CancellationToken ct = default);
}
