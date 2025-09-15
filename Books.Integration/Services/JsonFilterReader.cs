using Books.Core.Interfaces;
using Books.Core.Models.DTO;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Books.Infrastructure.Services;

public class JsonFilterReader : IFilterReader
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };

    private readonly IFileReader _fileReader;
    private readonly ILogger<JsonFilterReader> _logger;

    public JsonFilterReader(ILogger<JsonFilterReader> logger, IFileReader fileReader)
    {
        _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Filter Read(string filterPath)
    {
        if (string.IsNullOrWhiteSpace(filterPath))
        {
            throw new ArgumentException("Path is empty or whitespace.", nameof(filterPath));
        }

        _logger.LogInformation("Reading filter from {Path}", filterPath);

        string jsonString = _fileReader.ReadAllText(filterPath);
        Filter? filter;
        try
        {
            filter = JsonSerializer.Deserialize<Filter>(jsonString, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize filter.json.");
            throw new InvalidOperationException("The filter.json content is invalid.", ex);
        }

        ArgumentNullException.ThrowIfNull(filter);
        ValidateFilter(filter);

        return filter;
    }

    private static void ValidateFilter(Filter filter)
    {
        if ((filter.MoreThanPages.HasValue && filter.LessThanPages.HasValue) &&
            (filter.MoreThanPages.Value >= filter.LessThanPages.Value))
        {
            throw new InvalidDataException("Invalid pages range: MoreThanPages must be less than LessThanPages.");
        }

        if ((filter.PublishedAfter.HasValue && filter.PublishedBefore.HasValue) &&
            (filter.PublishedAfter.Value >= filter.PublishedBefore.Value))
        {
            throw new InvalidDataException("Invalid dates range: PublishedAfter must be earlier than PublishedBefore.");
        }
    }
}
