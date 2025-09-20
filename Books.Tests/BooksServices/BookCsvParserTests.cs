using Books.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Books.Tests.BooksServices;

public class BookCsvParserTests
{
    [Fact]
    public void Parse_SkipHeader_TakeOnlyDataRows()
    {
        // Arrange
        var rows = new[]
        {
            "Title,Pages,Genre,ReleaseDate,Author,Publisher",
            "Book One,100,Fiction,2023-01-01,Author A,Publisher A",
        };
        var parser = CreateParser();

        // Act
        var result = parser.Parse(rows).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Book One", result[0].Title);
        Assert.Equal(100, result[0].Pages);
        Assert.Equal("Fiction", result[0].Genre);
        Assert.Equal(new DateTime(2023, 1, 1), result[0].ReleaseDate);
        Assert.Equal("Author A", result[0].Author);
        Assert.Equal("Publisher A", result[0].Publisher);
    }

    [Theory]
    [InlineData("The Catcher in the Rye,224,Coming of Age,1951-07-16,J.D. Salinger,Little, Brown and Company", "Little, Brown and Company")]
    [InlineData("Frankenstein,280,Horror,1818-01-01,Mary Shelley,Lackington, Hughes, Harding, Mavor & Jones", "Lackington, Hughes, Harding, Mavor & Jones")]
    public void Parse_PublisherWithCommaSpace_IsSingleField(string row, string expectedPublisher)
    {
        // Arrange
        var rows = new[]
        {
            "Title,Pages,Genre,ReleaseDate,Author,Publisher",
            row
        };
        var parser = CreateParser();

        // Act
        var result = parser.Parse(rows).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedPublisher, result[0].Publisher);
    }

    [Fact]
    public void Parse_WhenSpaceBeforeComma_ThenRowIsSkipped()
    {
        // Arrange
        var rows = new[]
        {
            "Title,Pages,Genre,ReleaseDate,Author,Publisher",
            "The Great Gatsby ,180,Fiction,1925-04-10 ,F. Scott Fitzgerald,Charles Scribner's Sons"
        };
        var parser = CreateParser();

        // Act
        var result = parser.Parse(rows).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_RowWithIncorrectFieldCount_IsSkipped()
    {
        // Arrange
        var rows = new[]
        {
            "Title,Pages,Genre,ReleaseDate,Author,Publisher",
            "The Alchemist,197,Adventure,1988-04-25,Paulo Coelho" // no Publisher
        };
        var parser = CreateParser();

        // Act
        var result = parser.Parse(rows).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_InvalidDateFormat_IsSkipped()
    {
        // Arrange (date isn't in format "yyyy-MM-dd")
        var rows = new[]
        {
            "Title,Pages,Genre,ReleaseDate,Author,Publisher",
            "To Kill a Mockingbird,336,Fiction,07/11/1960,Harper Lee,HarperCollins"
        };
        var parser = CreateParser();

        // Act
        var result = parser.Parse(rows).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_NonIntegerPages_IsSkipped()
    {
        // Arrange
        var rows = new[]
        {
            "Title,Pages,Genre,ReleaseDate,Author,Publisher",
            "Brave New World,not-a-number,Science Fiction,1932-10-14,Aldous Huxley,Harper & Brothers"
        };
        var parser = CreateParser();

        // Act
        var result = parser.Parse(rows).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_NegativePages_IsSkipped()
    {
        // Arrange
        var rows = new[]
        {
            "Title,Pages,Genre,ReleaseDate,Author,Publisher",
            "Test Book,-10,Fiction,2000-01-01,Some Author,Some Publisher"
        };
        var parser = CreateParser();

        // Act
        var result = parser.Parse(rows).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_EmptyOrWhitespaceRow_IsSkipped()
    {
        // Arrange
        var rows = new[]
        {
            "Title,Pages,Genre,ReleaseDate,Author,Publisher",
            "   "
        };
        var parser = CreateParser();

        // Act
        var result = parser.Parse(rows).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_MultipleValidRows_ReturnsAll()
    {
        // Arrange
        var rows = new[]
        {
            "Title,Pages,Genre,ReleaseDate,Author,Publisher",
            "1984,328,Science Fiction,1949-06-08,George Orwell,Signet Classics",
            "Pride and Prejudice,432,Romance,1813-01-28,Jane Austen,Penguin Classics"
        };
        var parser = CreateParser();

        // Act
        var result = parser.Parse(rows).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, b => b.Title == "1984" && b.Pages == 328);
        Assert.Contains(result, b => b.Title == "Pride and Prejudice" && b.Pages == 432);
    }

    [Fact]
    public void Parse_OnlyHeaderRow_ReturnsEmptyCollection()
    {
        // Arrange
        var rows = new[]
        {
            "Title,Pages,Genre,ReleaseDate,Author,Publisher"
        };
        var parser = CreateParser();

        // Act
        var result = parser.Parse(rows).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_NonHeaderRowOrIncorrect_ThrowsInvalidDataException()
    {
        // Arrange
        var rows1 = new[]
        {
            "Wrong,Header,Row"
        };
        var rows2 = new[]
        {
            "Title,Pages,Genre,ReleaseDate,Author" // Missing Publisher
        };
        var parser = CreateParser();

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => parser.Parse(rows1).ToList());
        Assert.Throws<InvalidDataException>(() => parser.Parse(rows2).ToList());
    }

    private static BookCsvParser CreateParser()
    {
        return new BookCsvParser(new NullLogger<BookCsvParser>());
    }
}
