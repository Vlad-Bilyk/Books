using Books.Core.Models.DTO;

namespace Books.Core.Interfaces;

/// <summary>
/// Parses CSV rows into book DTOs.
/// </summary>
public interface IBookParser
{
    /// <summary>
    /// Parses lines (including header) into <see cref="BookCsvRow"/> sequence.
    /// </summary>
    /// <param name="rows">CSV lines with the first line as header.</param>
    /// <returns>Parsed rows; invalid lines may be skipped by implementation.</returns>
    public IEnumerable<BookCsvRow> Parse(IEnumerable<string> rows);
}
