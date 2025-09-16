using Books.Core.Models.DTO;

namespace Books.Core.Interfaces;

/// <summary>
/// Executes filtered book queries.
/// </summary>
public interface IBookSearchService
{
    /// <summary>
    /// Searches books using the provided filter.
    /// </summary>
    /// <param name="filter">Filter criteria (Title, Author, dates, etc.).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Matching books.</returns>
    Task<List<Models.Entities.Book>> SearchBooksAsync(Filter filter, CancellationToken ct = default);
}
