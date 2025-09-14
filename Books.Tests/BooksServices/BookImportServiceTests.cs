using Books.Core.Interfaces;
using Books.Core.Models.DTO;
using Books.Core.Models.Entities;
using Books.Infrastructure.Data;
using Books.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Books.Tests.BooksServices;
public class BookImportServiceTests
{
    [Fact]
    public async Task ImportFileAsync_WhenPathIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateDbContext(nameof(ImportFileAsync_WhenPathIsEmpty_ThrowsArgumentException));
        var fileReader = new Mock<IFileReader>(MockBehavior.Strict);
        var parser = new Mock<IBookParser>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger<BookImportService>>();
        var service = new BookImportService(context, fileReader.Object, parser.Object, logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.ImportFileAsync(string.Empty));
    }

    [Fact]
    public async Task ImportFileAsync_WhenValidRows_InsertsBooksAndRelatedEntities()
    {
        // Arrange
        using var context = CreateDbContext(nameof(ImportFileAsync_WhenValidRows_InsertsBooksAndRelatedEntities));
        var fileReader = new Mock<IFileReader>();
        fileReader.Setup(fr => fr.ReadLines("books.csv")).Returns(["header", "data"]);

        var rows = new[]
        {
            new ParsedBookRow
            {
                Title = "Dune",
                Pages = 412,
                Genre = "Sci-Fi",
                ReleaseDate = new DateTime(1965, 8, 1),
                Author = "Frank Herbert",
                Publisher = "Ace Books"
            }
        };

        var parser = CreateParserMockReturning(rows);
        var logger = Mock.Of<ILogger<BookImportService>>();
        var service = new BookImportService(context, fileReader.Object, parser.Object, logger);

        // Act
        var result = await service.ImportFileAsync("books.csv");

        // Assert
        Assert.Equal(1, result.Added);
        Assert.Equal(0, result.SkippedDuplicates);

        var saved = await context.Books.Include(b => b.Author).Include(b => b.Genre).Include(b => b.Publisher).ToListAsync();
        Assert.Single(saved);
        Assert.Equal("Dune", saved[0].Title);
        Assert.Equal(412, saved[0].Pages);

        Assert.Equal(1, await context.Genres.CountAsync());
        Assert.Equal(1, await context.Authors.CountAsync());
        Assert.Equal(1, await context.Publishers.CountAsync());
    }

    [Fact]
    public async Task ImportFileAsync_WhenSameFileContainsDuplicates_SkipsLocalDuplicates()
    {
        // Arrange
        using var context = CreateDbContext(nameof(ImportFileAsync_WhenSameFileContainsDuplicates_SkipsLocalDuplicates));
        var fileReader = new Mock<IFileReader>();
        fileReader.Setup(fr => fr.ReadLines("books.csv")).Returns(["header", "data1", "data2"]);

        var dune = new ParsedBookRow
        {
            Title = "Dune",
            Pages = 412,
            Genre = "Sci-Fi",
            ReleaseDate = new DateTime(1965, 8, 1),
            Author = "Frank Herbert",
            Publisher = "Ace Books"
        };

        var rows = new[] { dune, dune };
        var parser = CreateParserMockReturning(rows);
        var logger = Mock.Of<ILogger<BookImportService>>();

        var service = new BookImportService(context, fileReader.Object, parser.Object, logger);

        // Act
        var result = await service.ImportFileAsync("books.csv");

        // Assert
        Assert.Equal(1, result.Added);
        Assert.Equal(1, result.SkippedDuplicates);
        Assert.Equal(1, await context.Books.CountAsync());
    }

    [Fact]
    public async Task ImportFileAsync_WhenDbAlreadyHasBook_SkipsDuplicatesFromDatabase()
    {
        // Arrange
        using var context = CreateDbContext(nameof(ImportFileAsync_WhenDbAlreadyHasBook_SkipsDuplicatesFromDatabase));

        var author = new Author { Id = Guid.NewGuid(), Name = "Frank Herbert" };
        var genre = new Genre { Id = Guid.NewGuid(), Name = "Sci-Fi" };
        var publisher = new Publisher { Id = Guid.NewGuid(), Name = "Ace Books" };
        var existing = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Dune",
            Pages = 412,
            GenreId = genre.Id,
            ReleaseDate = new DateTime(1965, 8, 1),
            AuthorId = author.Id,
            PublisherId = publisher.Id
        };

        context.Genres.Add(genre);
        context.Authors.Add(author);
        context.Publishers.Add(publisher);
        context.Books.Add(existing);
        await context.SaveChangesAsync();

        var fileReader = new Mock<IFileReader>();
        fileReader.Setup(fr => fr.ReadLines("books.csv")).Returns(["header", "data"]);

        var rows = new[]
        {
            new ParsedBookRow
            {
                Title = "Dune",
                Pages = 412,
                Genre = "Sci-Fi",
                ReleaseDate = new DateTime(1965, 8, 1),
                Author = "Frank Herbert",
                Publisher = "Ace Books"
            }
        };
        var parser = CreateParserMockReturning(rows);
        var logger = Mock.Of<ILogger<BookImportService>>();

        var service = new BookImportService(context, fileReader.Object, parser.Object, logger);

        // Act
        var result = await service.ImportFileAsync("books.csv");

        // Assert
        Assert.Equal(0, result.Added);
        Assert.Equal(1, result.SkippedDuplicates);
        Assert.Equal(1, await context.Books.CountAsync());
    }

    [Fact]
    public async Task ImportFileAsync_WhenMultipleRowsShareSameNames_CreatesSingleAuthorGenrePublisher()
    {
        // Arrange
        using var context = CreateDbContext(nameof(ImportFileAsync_WhenMultipleRowsShareSameNames_CreatesSingleAuthorGenrePublisher));
        var fileReader = new Mock<IFileReader>();
        fileReader.Setup(fr => fr.ReadLines("books.csv")).Returns(["header", "data1", "data2"]);

        var rows = new[]
        {
            new ParsedBookRow
            {
                Title = "Dune",
                Pages = 412,
                Genre = "Sci-Fi",
                ReleaseDate = new DateTime(1965, 8, 1),
                Author = "Frank Herbert",
                Publisher = "Ace Books"
            },
            new ParsedBookRow
            {
                Title = "Dune Messiah",
                Pages = 256,
                Genre = "Sci-Fi",
                ReleaseDate = new DateTime(1969, 10, 15),
                Author = "Frank Herbert",
                Publisher = "Ace Books"
            }
        };

        var parser = CreateParserMockReturning(rows);
        var logger = Mock.Of<ILogger<BookImportService>>();

        var service = new BookImportService(context, fileReader.Object, parser.Object, logger);

        // Act
        var result = await service.ImportFileAsync("books.csv");

        // Assert
        Assert.Equal(2, result.Added);
        Assert.Equal(0, result.SkippedDuplicates);

        Assert.Equal(1, await context.Authors.CountAsync());
        Assert.Equal(1, await context.Genres.CountAsync());
        Assert.Equal(1, await context.Publishers.CountAsync());
    }

    [Fact]
    public async Task ImportFileAsync_WhenTokenAlreadyCanceled_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = CreateDbContext(nameof(ImportFileAsync_WhenTokenAlreadyCanceled_ThrowsOperationCanceledException));
        var fileReader = new Mock<IFileReader>();
        fileReader.Setup(r => r.ReadLines("books.csv")).Returns(["header", "d1", "d2", "d3"]);

        var rows = new[]
            {
                new ParsedBookRow { Title = "A", Pages = 100, Genre = "G", Author = "Au", Publisher = "Pu", ReleaseDate = new DateTime(2000,1,1) },
                new ParsedBookRow { Title = "B", Pages = 100, Genre = "G", Author = "Au", Publisher = "Pu", ReleaseDate = new DateTime(2000,1,1) },
                new ParsedBookRow { Title = "C", Pages = 100, Genre = "G", Author = "Au", Publisher = "Pu", ReleaseDate = new DateTime(2000,1,1) }
            };
        var parser = CreateParserMockReturning(rows);

        var service = new BookImportService(context, fileReader.Object, parser.Object, Mock.Of<ILogger<BookImportService>>());
        var canceled = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.ImportFileAsync("books.csv", canceled));
    }

    [Fact]
    public async Task ImportFileAsync_WhenSaveChangesThrowsDbUpdateException_LogsAndCountsAsSkippedDuplicate()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nameof(ImportFileAsync_WhenSaveChangesThrowsDbUpdateException_LogsAndCountsAsSkippedDuplicate))
            .Options;

        using var context = new ThrowingDbContext(options);
        var fileReader = new Mock<IFileReader>();
        fileReader.Setup(fr => fr.ReadLines("books.csv")).Returns(["header", "data"]);

        var rows = new[]
        {
            new ParsedBookRow
            {
                Title = "Dune",
                Pages = 412,
                Genre = "Sci-Fi",
                ReleaseDate = new DateTime(1965, 8, 1),
                Author = "Frank Herbert",
                Publisher = "Ace Books"
            }
        };
        var parser = CreateParserMockReturning(rows);
        var logger = new Mock<ILogger<BookImportService>>();

        var service = new BookImportService(context, fileReader.Object, parser.Object, logger.Object);

        // Act
        var result = await service.ImportFileAsync("books.csv");

        // Assert
        Assert.Equal(0, result.Added);
        Assert.Equal(1, result.SkippedDuplicates);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Title,Pages,Genre,ReleaseDate,Author,Publisher")]
    public async Task ImportFileAsync_WhenFileIsEmptyOrOnlyHeaderPresent_ReturnsZeroes(string row)
    {
        // Arrange
        using var context = CreateDbContext(nameof(ImportFileAsync_WhenFileIsEmptyOrOnlyHeaderPresent_ReturnsZeroes));
        var filereader = new Mock<IFileReader>();
        filereader.Setup(fr => fr.ReadLines("books.csv")).Returns([row]);

        var parser = CreateParserMockReturning([]);
        var service = new BookImportService(context, filereader.Object, parser.Object, Mock.Of<ILogger<BookImportService>>());

        // Act
        var result = await service.ImportFileAsync("books.csv");

        // Assert
        Assert.Equal(0, result.Added);
        Assert.Equal(0, result.SkippedDuplicates);
    }

    [Fact]
    public async Task ImportFileAsync_WhenExtensionIsNotCsv_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateDbContext(nameof(ImportFileAsync_WhenExtensionIsNotCsv_ThrowsArgumentException));
        var fileReader = new Mock<IFileReader>(MockBehavior.Strict);
        var parser = new Mock<IBookParser>(MockBehavior.Strict);

        var service = new BookImportService(context, fileReader.Object, parser.Object, Mock.Of<ILogger<BookImportService>>());

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.ImportFileAsync("filter.json"));

        // Assert
        Assert.Contains(".csv", ex.Message, StringComparison.OrdinalIgnoreCase);

        fileReader.Verify(r => r.ReadLines(It.IsAny<string>()), Times.Never);
        parser.Verify(p => p.Parse(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    // Helpers
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    private static Mock<IBookParser> CreateParserMockReturning(IEnumerable<ParsedBookRow> rows)
    {
        var parser = new Mock<IBookParser>(MockBehavior.Strict);
        parser.Setup(p => p.Parse(It.IsAny<IEnumerable<string>>()))
            .Returns(rows);
        return parser;
    }

    private sealed class ThrowingDbContext : AppDbContext
    {
        public ThrowingDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new DbUpdateException("Simulated duplicate via unique index.");
        }
    }
}
