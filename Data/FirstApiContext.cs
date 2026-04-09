
using FirstApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstApi.Data;

public class FirstApiContext(DbContextOptions<FirstApiContext> options) : DbContext(options)
{
protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Book>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<User> Users { get; set; }


}