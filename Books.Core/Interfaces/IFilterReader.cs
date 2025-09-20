using Books.Core.Models.DTO;

namespace Books.Core.Interfaces;

/// <summary>
/// Loads filter settings from a JSON file.
/// </summary>
public interface IFilterReader
{
    /// <summary>
    /// Reads and validates a filter from the given path.
    /// </summary>
    /// <param name="filterPath">Path to <c>filter.json</c>.</param>
    /// <returns>Deserialized filter.</returns>
    Filter Read(string filterPath);
}
