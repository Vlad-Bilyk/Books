using Books.Core.Models.DTO;
using Books.Infrastructure.Services;
using Books.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Books.Tests.BooksServices;

public class BookSearchServiceTests
{
    #region Basics
    [Fact]
    public async Task SearchedBooksAsync_WhenFilterIsNull_Throws()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            var service = new BookSearchService(db);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await service.SearchBooksAsync(null!));
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task SearchBooksAsync_WhenFilterEmpty_ReturnsAll()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);
            var filter = new Filter();

            var result = await service.SearchBooksAsync(filter);

            Assert.Equal(await db.Books.CountAsync(), result.Count);
            var expected = await db.Books.AsNoTracking()
                .OrderBy(b => b.Title)
                .Select(b => b.Title).ToListAsync();
            var actual = result.Select(b => b.Title).ToList();
            Assert.Equal(expected, actual);
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task SearchBooksAsync_WhenNoMatches_ReturnsEmpty()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var filter = new Filter { Title = "NonexistentTitle" };
            var result = await service.SearchBooksAsync(filter);

            Assert.Empty(result);
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    #endregion

    #region String filters (Title/Genre/Author/Publisher)
    [Fact]
    public async Task SearchBooksAsync_WhenTitleContainsSubstring_ReturnsMatches()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var filter = new Filter { Title = "War" };

            var result = await service.SearchBooksAsync(filter);
            var titles = result.Select(b => b.Title).ToList();

            Assert.Contains("War and Peace", titles);
            Assert.Contains("The Art of War", titles);
            Assert.DoesNotContain("Nineteen Eighty-Four", titles);
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task SearchBooksAsync_WhenTitleHasWhitespace_TrimmedAndApplied()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var filter = new Filter { Title = "  War  " };

            var result = await service.SearchBooksAsync(filter);
            var titles = result.Select(b => b.Title).ToList();

            Assert.Contains("War and Peace", titles);
            Assert.Contains("The Art of War", titles);
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task SearchBooksAsync_WhenGenreContainsSubstring_ReturnsMatches()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var filter = new Filter { Genre = "Novel" };
            var result = await service.SearchBooksAsync(filter);

            Assert.All(result, b => Assert.Contains("Novel", b.Genre.Name));
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task SearchBooksAsync_WhenAuthorContainsSubstring_ReturnsMatches()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var filter = new Filter { Author = "Orwell" };
            var result = await service.SearchBooksAsync(filter);

            Assert.All(result, b => Assert.Contains("Orwell", b.Author.Name));
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task SearchBooksAsync_WhenPublisherContainsSubstring_ReturnsMatches()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var filter = new Filter { Publisher = "Penguin" };
            var result = await service.SearchBooksAsync(filter);

            Assert.All(result, b => Assert.Contains("Penguin", b.Publisher.Name));
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task SearchBooksAsync_WhenTitleIsWhitespace_IgnoresTitleCriterion()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var filter = new Filter { Title = "   " };
            var result = await service.SearchBooksAsync(filter);

            Assert.Equal(await db.Books.CountAsync(), result.Count);
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    #endregion

    #region Numeric filters (pages)
    [Fact]
    public async Task SearchBooksAsync_WhenMoreThanPages_AppliesExclusiveLowerBound()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var filter = new Filter { MoreThanPages = 100 };
            var result = await service.SearchBooksAsync(filter);

            Assert.DoesNotContain(result, b => b.Pages == 100);
            Assert.All(result, b => Assert.True(b.Pages > 100));
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task SearchBooksAsync_WhenLessThanPages_AppliesExclusiveUpperBound()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var filter = new Filter { LessThanPages = 100 };
            var result = await service.SearchBooksAsync(filter);

            Assert.DoesNotContain(result, b => b.Pages == 100);
            Assert.All(result, b => Assert.True(b.Pages < 100));
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task SearchBooksAsync_WhenPagesRange_AppliesExclusiveBounds()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var filter = new Filter { MoreThanPages = 99, LessThanPages = 101 };
            var result = await service.SearchBooksAsync(filter);

            var pages = result.Select(b => b.Pages).ToList();

            Assert.Contains(100, pages);
            Assert.DoesNotContain(99, pages);
            Assert.DoesNotContain(101, pages);
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }
    #endregion

    #region Date filters

    [Fact]
    public async Task SearchBooksAsync_WhenPublishedAfter_AppliesExclusiveLowerBound()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var after = new DateTime(2020, 12, 31);
            var filter = new Filter { PublishedAfter = after };

            var result = await service.SearchBooksAsync(filter);

            Assert.DoesNotContain(result, b => b.ReleaseDate == after);
            Assert.All(result, b => Assert.True(b.ReleaseDate > after));
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task SearchBooksAsync_WhenPublishedBefore_AppliesExclusiveUpperBound()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var before = new DateTime(2020, 1, 2);
            var filter = new Filter { PublishedBefore = before };

            var result = await service.SearchBooksAsync(filter);

            Assert.DoesNotContain(result, b => b.ReleaseDate == before);
            Assert.All(result, b => Assert.True(b.ReleaseDate < before));
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task SearchBooksAsync_WhenDateCorridor_AppliesExclusiveBounds()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var after = new DateTime(2019, 12, 31);
            var before = new DateTime(2020, 1, 2);
            var filter = new Filter { PublishedAfter = after, PublishedBefore = before };

            var result = await service.SearchBooksAsync(filter);
            var dates = result.Select(b => b.ReleaseDate).ToList();

            Assert.Single(dates);
            Assert.Contains(new DateTime(2020, 1, 1), dates);
            Assert.DoesNotContain(after, dates);
            Assert.DoesNotContain(before, dates);
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    #endregion

    #region Combined AND logic

    [Fact]
    public async Task SearchBooksAsync_WhenMultipleCriteria_UseAndLogic()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var filter = new Filter
            {
                Title = "War",
                MoreThanPages = 200,
                PublishedBefore = new DateTime(1930, 1, 1)
            };

            var result = await service.SearchBooksAsync(filter);
            var titles = result.Select(b => b.Title).ToList();

            Assert.Single(titles);
            Assert.Contains("War and Peace", titles[0]);
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    #endregion

    #region Other tests

    [Fact]
    public async Task SearchBooksAsync_WhenAnyFilter_AppliesOrderByTitlesAsc()
    {
        var (db, conn) = TestDb.CreateOpened();
        try
        {
            TestDataSeeder.Seed(db);
            var service = new BookSearchService(db);

            var filter = new Filter { Title = "Leo Tolstoy" };

            var result = await service.SearchBooksAsync(filter);
            var actual = result.Select(b => b.Title).ToList();
            var expected = actual.OrderBy(t => t, StringComparer.Ordinal).ToList();

            Assert.Equal(expected, actual);
        }
        finally
        {
            await conn.DisposeAsync();
            await db.DisposeAsync();
        }
    }

    #endregion
}
