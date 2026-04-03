
using FirstApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstApi.Data;

public class FirstApiContext(DbContextOptions<FirstApiContext> options) : DbContext(options)
{
protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<Book>().HasData(
            new Book
            {
                Id = 1,
                Title = "The Great Gatsby",
                Author = "F. Scott Fitzgerald",
                YearPublished = 1925
            },
            new Book
            {
                Id = 2,
                Title = "To Kill a Mockingbird",
                Author = "Harper Lee",
                YearPublished = 1960
            },
            new Book
            {
                Id = 3,
                Title = "1984",
                Author = "George Orwell",
                YearPublished = 1949
            }
        );
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<User> Users { get; set; }


}