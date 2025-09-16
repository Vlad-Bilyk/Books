using Books.Core.Models.DTO;
using Books.Infrastructure.Services;
using Books.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace Books.Tests.BooksServices;

public class CsvExportWriterTests
{
    private const string _headerLine = "Title,Pages,Genre,ReleaseDate,Author,Publisher";

    [Fact]
    public async Task WriteAsync_NullRows_ThrowsArgumentNullException()
    {
        var writer = CreateWriter(new FakeTimeProvider(DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<ArgumentNullException>(() => writer.WriteAsync(null!));
    }

    [Fact]
    public async Task WriteAsync_EmptyRows_CreatesFileWithHeaderOnly_And_UsesTimestampInName()
    {
        using var tempDir = new TempDir();
        var fixedTime = new FakeTimeProvider(new DateTimeOffset(2025, 09, 16, 12, 34, 56, TimeSpan.Zero));
        var writer = CreateWriter(fixedTime);

        var path = await writer.WriteAsync([], tempDir.Path);

        Assert.True(File.Exists(path));

        var fileName = Path.GetFileName(path);
        Assert.Equal("books_20250916_123456Z.csv", fileName);

        var lines = await File.ReadAllLinesAsync(path);
        Assert.Single(lines);
        Assert.Equal(_headerLine, lines[0]);
    }

    [Fact]
    public async Task WriteAsync_WriteRows_WithExactFormat()
    {
        using var tempDir = new TempDir();
        var fixedTime = new FakeTimeProvider(new DateTimeOffset(2025, 01, 01, 00, 00, 00, TimeSpan.Zero));
        var writer = CreateWriter(fixedTime);

        var rows = new[]
        {
            new BookCsvRow
            {
                Title = "A",
                Pages = 10,
                Genre = "G",
                ReleaseDate = new DateTime(2020, 1, 2),
                Author = "Auth",
                Publisher = "Pub"
            },
            new BookCsvRow
            {
                Title = "B",
                Pages = 11,
                Genre = "H",
                ReleaseDate = new DateTime(2021, 2, 3),
                Author = "Auth2",
                Publisher = "Pub2"
            }
        };

        var path = await writer.WriteAsync(rows, tempDir.Path);
        var lines = await File.ReadAllLinesAsync(path);

        Assert.Equal(3, lines.Length);
        Assert.Equal(_headerLine, lines[0]);
        Assert.Equal("A,10,G,2020-01-02,Auth,Pub", lines[1]);
        Assert.Equal("B,11,H,2021-02-03,Auth2,Pub2", lines[2]);
    }

    [Fact]
    public async Task WriteAsync_NullOutputDir_WritesToAppBaseDirectory()
    {
        var fixedTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var writer = CreateWriter(fixedTime);

        var rows = new[]
        {
            new BookCsvRow
            {
                Title = "T",
                Pages = 1,
                Genre = "G",
                ReleaseDate = new DateTime(2000, 1, 1),
                Author = "A",
                Publisher = "P"
            }
        };

        var path = await writer.WriteAsync(rows, null);
        var actualDir = Path.GetDirectoryName(Path.GetFullPath(path));
        var expectedDir = Path.GetFullPath(AppContext.BaseDirectory);

        static string Norm(string p) =>
            p.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        Assert.False(string.IsNullOrWhiteSpace(actualDir));
        Assert.Equal(Norm(actualDir), Norm(expectedDir), ignoreCase: true);

        File.Delete(path);
    }

    [Fact]
    public async Task WriteAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var tempDir = new TempDir();
        var fixedTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var writer = CreateWriter(fixedTime);

        var rows = new[]
        {
            new BookCsvRow
            {
                Title = "T",
                Pages = 1,
                Genre = "G",
                ReleaseDate = new DateTime(2000, 1, 1),
                Author = "A",
                Publisher = "P"
            }
        };

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            writer.WriteAsync(rows, tempDir.Path, cts.Token));
    }

    private static CsvExportWriter CreateWriter(FakeTimeProvider timeProvider)
    {
        return new CsvExportWriter(NullLogger<CsvExportWriter>.Instance, timeProvider);
    }
}
