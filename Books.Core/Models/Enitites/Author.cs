namespace Books.Core.Models.Enitites;

public class Author
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public ICollection<Book> Books { get; } = [];
}
