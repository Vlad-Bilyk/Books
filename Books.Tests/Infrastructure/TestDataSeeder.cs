using Books.Core.Models.Entities;
using Books.Infrastructure.Data;

namespace Books.Tests.Infrastructure;

internal static class TestDataSeeder
{
    public static void Seed(AppDbContext db)
    {
        var gNovel = new Genre { Name = "Novel" };
        var gSciFi = new Genre { Name = "Sci-Fi" };
        var gPoetry = new Genre { Name = "Poetry" };

        var aTolstoy = new Author { Name = "Leo Tolstoy" };
        var aOrwell = new Author { Name = "George Orwell" };
        var aPoe = new Author { Name = "Edgar Allan Poe" };

        var pMessenger = new Publisher { Name = "The Russian Messenger" };
        var pSecker = new Publisher { Name = "Secker & Warburg" };
        var pPenguin = new Publisher { Name = "Penguin" };

        var books = new[]
        {
            new Book
            {
                Title = "War and Peace", Genre = gNovel, Author = aTolstoy, Publisher = pMessenger,
                Pages = 1225, ReleaseDate = new DateTime(1869, 1, 1)
            },
            new Book
            {
                Title = "The Art of War", Genre = gPoetry, Author = aPoe, Publisher = pPenguin,
                Pages = 200, ReleaseDate = new DateTime(1910, 1, 1)
            },
            new Book
            {
                Title = "Nineteen Eighty-Four", Genre = gSciFi, Author = aOrwell, Publisher = pSecker,
                Pages = 328, ReleaseDate = new DateTime(1949, 6, 8)
            },
            new Book
            {
                Title = "Alpha", Genre = gNovel, Author = aTolstoy, Publisher = pPenguin,
                Pages = 99, ReleaseDate = new DateTime(2019, 12, 31)
            },
            new Book
            {
                Title = "Bravo", Genre = gNovel, Author = aTolstoy, Publisher = pPenguin,
                Pages = 100, ReleaseDate = new DateTime(2020, 1, 1)
            },
            new Book
            {
                Title = "Charlie", Genre = gNovel, Author = aTolstoy, Publisher = pPenguin,
                Pages = 101, ReleaseDate = new DateTime(2020, 1, 2)
            },
            new Book
            {
                Title = "Beta", Genre = gPoetry, Author = aPoe, Publisher = pPenguin,
                Pages = 150, ReleaseDate = new DateTime(1850, 1, 1)
            },
            new Book
            {
                Title = "Gamma", Genre = gSciFi, Author = aOrwell, Publisher = pSecker,
                Pages = 450, ReleaseDate = new DateTime(1950, 1, 1)
            }
        };

        db.Books.AddRange(books);
        db.SaveChanges();
    }
}
