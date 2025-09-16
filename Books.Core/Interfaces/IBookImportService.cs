using Books.Core.Models.DTO;

namespace Books.Core.Interfaces;

/// <summary>
/// Imports books from a CSV file into the data store.
/// </summary>
public interface IBookImportService
{
    /// <summary>
    /// Imports a CSV file.
    /// </summary>
    /// <param name="filePath">CSV file path.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Import result with counters.</returns>
    Task<ImportResult> ImportFileAsync(string filePath, CancellationToken ct = default);
}
