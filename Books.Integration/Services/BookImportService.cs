using Books.Core.Interfaces;
using Books.Core.Models.DTO;
using Books.Core.Models.Entities;
using Books.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Books.Infrastructure.Services;

public class BookImportService : IBookImportService
{
    private readonly AppDbContext _db;
    private readonly IFileReader _fileReader;
    private readonly IBookParser _csvParser;
    private readonly ILogger<BookImportService> _logger;

    public BookImportService(AppDbContext db, IFileReader fileReader,
        IBookParser csvParser, ILogger<BookImportService> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
        _csvParser = csvParser ?? throw new ArgumentNullException(nameof(csvParser));
        _logger = logger;
    }

    public async Task<ImportResult> ImportFileAsync(string filePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is empty.", nameof(filePath));
        }

        if (!string.Equals(Path.GetExtension(filePath), ".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Only .csv files are supported.");
        }

        var lines = _fileReader.ReadLines(filePath);

        if (lines is null || !lines.Skip(1).Any())
        {
            return new ImportResult();
        }

        var rows = _csvParser.Parse(lines);
        return await ImportAsync(rows, ct);
    }

    private async Task<ImportResult> ImportAsync(IEnumerable<ParsedBookRow> books, CancellationToken ct = default)
    {
        var result = new ImportResult();

        var authors = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var genres = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var publishers = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        var localBookKeys = new HashSet<string>();
        var toInsert = new List<Book>();

        foreach (var book in books)
        {
            ct.ThrowIfCancellationRequested();

            var genreId = await TryGetIdAsync(_db.Genres, book.Genre, genres, ct)
                ?? CreateEntity(_db.Genres, book.Genre, genres,
                (id, name) => new Genre { Id = id, Name = name });

            var authorId = await TryGetIdAsync(_db.Authors, book.Author, authors, ct)
                ?? CreateEntity(_db.Authors, book.Author, authors,
                (id, name) => new Author { Id = id, Name = name });

            var publisherId = await TryGetIdAsync(_db.Publishers, book.Publisher, publishers, ct)
                ?? CreateEntity(_db.Publishers, book.Publisher, publishers,
                (id, name) => new Publisher { Id = id, Name = name });

            // Catch book duplicate
            var key = MakeBusinessKey(book.Title, authorId, publisherId, book.ReleaseDate);
            if (!localBookKeys.Add(key))
            {
                result.SkippedDuplicates++;
                continue;
            }

            var exists = await _db.Books.AsNoTracking().AnyAsync(
                b => b.Title == book.Title &&
                b.AuthorId == authorId &&
                b.PublisherId == publisherId &&
                b.ReleaseDate == book.ReleaseDate, ct);

            if (exists)
            {
                result.SkippedDuplicates++;
                continue;
            }

            toInsert.Add(BuildBookEntity(book, genreId, authorId, publisherId));
        }

        if (toInsert.Count > 0)
        {
            _db.Books.AddRange(toInsert);

            try
            {
                await _db.SaveChangesAsync(ct);
                result.Added += toInsert.Count;
            }
            catch (DbUpdateException ex)
            {
                result.SkippedDuplicates++;
                _logger.LogWarning(ex, "Duplicates detected during SaveChanges (unique index).");
            }
        }

        return result;
    }

    private static async Task<Guid?> TryGetIdAsync<T>(DbSet<T> set, string name,
        Dictionary<string, Guid> cache, CancellationToken ct = default) where T : class
    {
        if (cache.TryGetValue(name, out Guid cacheId))
        {
            return cacheId;
        }

        var dbId = await set.AsNoTracking()
            .Where(e => EF.Property<string>(e, "Name") == name)
            .Select(e => EF.Property<Guid>(e, "Id"))
            .FirstOrDefaultAsync(ct);

        if (dbId != Guid.Empty)
        {
            cache[name] = dbId;
            return dbId;
        }

        return null;
    }

    private static Guid CreateEntity<T>(DbSet<T> set, string name,
        Dictionary<string, Guid> cache, Func<Guid, string, T> factory) where T : class
    {
        var id = Guid.NewGuid();
        var entity = factory(id, name);
        set.Add(entity);
        cache[name] = id;
        return id;
    }

    private static Book BuildBookEntity(ParsedBookRow book, Guid genreId, Guid authorId, Guid publisherId)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Title = book.Title,
            Pages = book.Pages,
            GenreId = genreId,
            AuthorId = authorId,
            PublisherId = publisherId,
            ReleaseDate = book.ReleaseDate,
        };
    }

    private static string MakeBusinessKey(string title, Guid authorId, Guid publisherId, DateTime releaseDate)
    {
        return $"{title}|{authorId}|{publisherId}|{releaseDate}";
    }
}
