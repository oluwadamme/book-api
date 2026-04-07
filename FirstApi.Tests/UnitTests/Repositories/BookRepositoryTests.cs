using FirstApi.Repositories;
using FirstApi.Models;
using FirstApi.Data;
using Microsoft.EntityFrameworkCore;

namespace FirstApi.Tests.UnitTests.Repositories;

public class BookRepositoryTests
{
    // Helper method to create a fresh in-memory database for each test
    private static FirstApiContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<FirstApiContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique DB per test
            .Options;

        return new FirstApiContext(options);
    }

    [Fact]
    public async Task GetAllBooksAsync_ReturnsAllBooks_WhenCalledWithValidUserId()
    {
        // Arrange
        var context = CreateInMemoryContext();

        // Seed test data directly into the in-memory database
        context.Books.AddRange(
            new Book { Id = 1, UserId = 1, Title = "Book A", Author = "Author A", YearPublished = 2020 },
            new Book { Id = 2, UserId = 1, Title = "Book B", Author = "Author B", YearPublished = 2021 },
            new Book { Id = 3, UserId = 2, Title = "Book C", Author = "Author C", YearPublished = 2022 }
        );
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetAllBooksAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);                  // only userId=1's books
        Assert.All(result, book => Assert.Equal(1, book.UserId));
    }

    [Fact]
    public async Task GetAllBooksAsync_ReturnsEmptyList_WhenUserHasNoBooks()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetAllBooksAsync(999);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBookByIdAsync_ReturnsBook_WhenBookExistsAndBelongsToUser()
    {
        // Arrange
        var context = CreateInMemoryContext();
        context.Books.Add(new Book { Id = 1, UserId = 5, Title = "My Book", Author = "Me", YearPublished = 2024 });
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetBookByIdAsync(1, 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("My Book", result.Title);
    }

    [Fact]
    public async Task GetBookByIdAsync_ReturnsNull_WhenBookBelongsToDifferentUser()
    {
        // Arrange
        var context = CreateInMemoryContext();
        context.Books.Add(new Book { Id = 1, UserId = 5, Title = "My Book", Author = "Me", YearPublished = 2024 });
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act — asking for userId=999, but book belongs to userId=5
        var result = await repository.GetBookByIdAsync(1, 999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddBookAsync_SavesBookToDatabase()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new BookRepository(context);
        var book = new Book { Title = "New Book", Author = "Author", YearPublished = 2025, UserId = 1 };

        // Act
        var result = await repository.AddBookAsync(book);

        // Assert
        Assert.True(result.Id > 0);  // EF should assign an ID
        Assert.Equal(1, await context.Books.CountAsync());
    }

    [Fact]
    public async Task DeleteBookAsync_RemovesBookFromDatabase()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var book = new Book { Id = 1, UserId = 1, Title = "To Delete", Author = "Author", YearPublished = 2020 };
        context.Books.Add(book);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act
        await repository.DeleteBookAsync(book);

        // Assert
        Assert.Equal(0, await context.Books.CountAsync());
    }
}