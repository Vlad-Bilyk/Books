using Books.Core.Interfaces;
using Books.Core.Models.DTO;
using Books.Core.Models.Entities;
using Books.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Books.Infrastructure.Services;

public class BookSearchService : IBookSearchService
{
    private readonly AppDbContext _db;

    public BookSearchService(AppDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<List<Book>> SearchBooksAsync(Filter filter, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        static string? Normalize(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        var title = Normalize(filter.Title);
        var genre = Normalize(filter.Genre);
        var author = Normalize(filter.Author);
        var publisher = Normalize(filter.Publisher);

        IQueryable<Book> query = _db.Books.AsNoTracking()
            .Include(b => b.Genre)
            .Include(b => b.Author)
            .Include(b => b.Publisher);

        if (title is not null)
        {
            query = query.Where(b => b.Title.Contains(title));
        }
        if (genre is not null)
        {
            query = query.Where(b => b.Genre.Name.Contains(genre));
        }
        if (author is not null)
        {
            query = query.Where(b => b.Author.Name.Contains(author));
        }
        if (publisher is not null)
        {
            query = query.Where(b => b.Publisher.Name.Contains(publisher));
        }

        if (filter.MoreThanPages is int minPages)
        {
            query = query.Where(b => b.Pages > minPages);
        }
        if (filter.LessThanPages is int maxPages)
        {
            query = query.Where(b => b.Pages < maxPages);
        }

        if (filter.PublishedAfter is DateTime after)
        {
            query = query.Where(b => b.ReleaseDate > after);
        }
        if (filter.PublishedBefore is DateTime before)
        {
            query = query.Where(b => b.ReleaseDate < before);
        }

        query = query.OrderBy(b => b.Title);

        return await query.ToListAsync(ct);
    }
}
