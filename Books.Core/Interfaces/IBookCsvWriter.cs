using Books.Core.Models.DTO;

namespace Books.Core.Interfaces;

/// <summary>
/// Writes book rows to a CSV file in the standard schema.
/// </summary>
public interface IBookCsvWriter
{
    /// <summary>
    /// Writes rows to CSV.
    /// </summary>
    /// <param name="foundBooks">Rows to write (Title, Pages, Genre, ReleaseDate, Author, Publisher).</param>
    /// <param name="outputDir">Target directory; if null/empty, the app folder is used.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Full path to the created CSV file.</returns>
    Task<string> WriteAsync(IEnumerable<BookCsvRow> foundBooks, string? outputDir = null, CancellationToken ct = default);
}
