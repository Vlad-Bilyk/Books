using Books.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Books.Infrastructure.Data;

/// <summary>
/// EF Core database context for the application data model.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Author> Authors { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Book> Books { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Book
        modelBuilder.Entity<Book>(b =>
        {
            b.Property(x => x.Title).IsRequired().HasMaxLength(450);

            b.HasOne(x => x.Genre)
                .WithMany()
                .HasForeignKey(x => x.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Author)
                .WithMany()
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Publisher)
                .WithMany()
                .HasForeignKey(x => x.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.Title, x.AuthorId, x.PublisherId, x.ReleaseDate })
                .IsUnique();
        });

        // Author
        modelBuilder.Entity<Author>(a =>
        {
            a.HasIndex(x => x.Name).IsUnique();
        });

        // Genre
        modelBuilder.Entity<Genre>(g =>
        {
            g.HasIndex(x => x.Name).IsUnique();
        });

        // Publisher
        modelBuilder.Entity<Publisher>(p =>
        {
            p.HasIndex(x => x.Name).IsUnique();
        });
    }
}
