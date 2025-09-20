using Books.Core.Interfaces;
using Books.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Books.Tests.BooksServices;

public class JsonFilterReaderTests
{
    private readonly Mock<IFileReader> _fileReaderMock;
    private readonly Mock<ILogger<JsonFilterReader>> _loggerMock;

    public JsonFilterReaderTests()
    {
        _fileReaderMock = new Mock<IFileReader>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<JsonFilterReader>>(MockBehavior.Loose);
    }

    [Fact]
    public void Read_WhenPathIsNullOrWhitespace_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.Read(null!));
        Assert.Throws<ArgumentException>(() => service.Read(string.Empty));
        Assert.Throws<ArgumentException>(() => service.Read("   "));
    }

    [Fact]
    public void Read_WhenFileReaderFails_PropagatesIOException()
    {
        // Arrange
        var service = CreateService();
        _fileReaderMock
            .Setup(fr => fr.ReadAllText(It.IsAny<string>()))
            .Throws(new IOException("Simulated I/O failure"));

        // Act & Assert
        Assert.Throws<IOException>(() => service.Read("filter.json"));
    }

    [Fact]
    public void Read_WhenJsonIsInvalid_ThrowsInvalidOperation()
    {
        // Arrange
        var service = CreateService();
        var invalidJson = """{"Title":"War"  "Author":"Tolstoy"}"""; // missing comma
        _fileReaderMock
            .Setup(fr => fr.ReadAllText(It.IsAny<string>()))
            .Returns(invalidJson);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => service.Read("filter.json"));
        Assert.Contains("invalid", ex.Message, StringComparison.OrdinalIgnoreCase);

        // Arrange
        _fileReaderMock.Verify(fr => fr.ReadAllText(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Read_WhenJsonIsLiteralNull_ThrowsArgumentNull()
    {
        // Arrange
        var service = CreateService();
        _fileReaderMock
            .Setup(fr => fr.ReadAllText(It.IsAny<string>()))
            .Returns("null");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.Read("filter.json"));
    }

    [Fact]
    public void Read_WhenJsonIsEmptyObject_ReturnsFilter()
    {
        // Arrange
        var service = CreateService();
        _fileReaderMock
            .Setup(fr => fr.ReadAllText(It.IsAny<string>()))
            .Returns("{}");

        // Act
        var filter = service.Read("filter.json");

        // Assert
        Assert.NotNull(filter);
        Assert.Null(filter.Title);
        Assert.Null(filter.Author);
        Assert.Null(filter.Genre);
        Assert.Null(filter.Publisher);
        Assert.Null(filter.MoreThanPages);
        Assert.Null(filter.LessThanPages);
        Assert.Null(filter.PublishedAfter);
        Assert.Null(filter.PublishedBefore);
    }

    [Fact]
    public void Read_WhenJsonIsValid_ReturnsFilter()
    {
        // Arrange
        var service = CreateService();
        var json = """
        {
            "Title": "1984",
            "Author": "George Orwell",
            "Genre": "Dystopian",
            "Publisher": "Secker & Warburg",
            "MoreThanPages": 200,
            "LessThanPages": 400,
            "PublishedAfter": "1940-01-01",
            "PublishedBefore": "1950-12-31"
        }
        """;
        _fileReaderMock
            .Setup(fr => fr.ReadAllText(It.IsAny<string>()))
            .Returns(json);

        // Act
        var filter = service.Read("filter.json");

        // Assert
        Assert.NotNull(filter);
        Assert.Equal("1984", filter.Title);
        Assert.Equal("George Orwell", filter.Author);
        Assert.Equal("Dystopian", filter.Genre);
        Assert.Equal("Secker & Warburg", filter.Publisher);
        Assert.Equal(200, filter.MoreThanPages);
        Assert.Equal(400, filter.LessThanPages);
        Assert.Equal(DateTime.Parse("1940-01-01"), filter.PublishedAfter);
        Assert.Equal(DateTime.Parse("1950-12-31"), filter.PublishedBefore);
    }

    [Theory]
    [InlineData(300, 200)] // MoreThanPages >= LessThanPages
    [InlineData(400, 400)] // MoreThanPages == LessThanPages
    public void Read_WhenPagesRangeIsInvalid_ThrowsInvalidData(int moreThan, int lessThan)
    {
        // Arrange
        var service = CreateService();
        var json = $$"""
        {
            "MoreThanPages": {{moreThan}},
            "LessThanPages": {{lessThan}}
        }
        """;
        _fileReaderMock
            .Setup(fr => fr.ReadAllText(It.IsAny<string>()))
            .Returns(json);

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => service.Read("filter.json"));
    }

    [Fact]
    public void Read_WhenDatesRangeIsInvalid_ThrowsInvalidData()
    {
        // Arrange
        var service = CreateService();
        var json = """
        {
            "PublishedAfter": "1950-01-01",
            "PublishedBefore": "1940-12-31"
        }
        """;
        _fileReaderMock
            .Setup(fr => fr.ReadAllText(It.IsAny<string>()))
            .Returns(json);

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => service.Read("filter.json"));
    }

    private JsonFilterReader CreateService()
    {
        return new JsonFilterReader(_loggerMock.Object, _fileReaderMock.Object);
    }
}
