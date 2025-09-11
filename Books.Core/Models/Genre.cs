namespace Books.Core.Models;

public class Genre
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public ICollection<Book> Books { get; } = [];
}
