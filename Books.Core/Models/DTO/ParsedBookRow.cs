namespace Books.Core.Models.DTO;

public class ParsedBookRow
{
    public string Title { get; set; } = string.Empty;
    public int Pages { get; set; }
    public string Genre { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
}
