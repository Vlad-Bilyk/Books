namespace Books.Core.Models.Enitites;

public class Book
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public int Pages { get; set; }
    public Guid GenreId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid PublisherID { get; set; }
    public DateTime ReleaseDate { get; set; }

    public Genre Genre { get; set; } = null!;
    public Author Author { get; set; } = null!;
    public Publishier Publishier { get; set; } = null!;
}
