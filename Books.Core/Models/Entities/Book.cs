namespace Books.Core.Models.Entities;

public class Book
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public int Pages { get; set; }
    public Guid GenreId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid PublisherId { get; set; }
    public DateTime ReleaseDate { get; set; }

    // Navigation properties
    public Genre Genre { get; set; } = null!;
    public Author Author { get; set; } = null!;
    public Publisher Publisher { get; set; } = null!;
}
