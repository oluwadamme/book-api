using Moq;
using FirstApi.Services;
using FirstApi.Models;
using FirstApi.Repositories.Interfaces;

namespace FirstApi.Tests.UnitTests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _bookService = new BookService(_mockBookRepository.Object);
    }

    [Fact]
    public async Task GetAllBooksAsync_ReturnsAllBooks_WhenCalledWithValidUserId()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book { Id = 1, UserId = 1, Title = "Book A", Author = "Author A", YearPublished = 2020 },
            new Book { Id = 2, UserId = 1, Title = "Book B", Author = "Author B", YearPublished = 2021 },
            new Book { Id = 3, UserId = 2, Title = "Book C", Author = "Author C", YearPublished = 2022 }
        };
        _mockBookRepository.Setup(r => r.GetAllBooksAsync(1)).ReturnsAsync(books);

        // Act
        var result = await _bookService.GetAllBooksAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }
    [Fact]
    public async Task AddBookAsync_ValidBook_SetsUserIdAndReturnsBook()
    {
        // Arrange
        var book = new Book { Title = "Test Book", Author = "Test Author", YearPublished = 2024 };
        var userId = 5;
        // Tell the mock: "When AddBookAsync is called with ANY Book, return that same book"
        _mockBookRepository
            .Setup(repo => repo.AddBookAsync(It.IsAny<Book>()))
            .ReturnsAsync((Book b) => b);  // returns whatever book was passed in
        // Act
        var result = await _bookService.AddBookAsync(book, userId);
        // Assert
        Assert.Equal(userId, result.UserId);        // Did it set the UserId?
        Assert.Equal("Test Book", result.Title);     // Is the title intact?
        // Verify the repository was actually called exactly once
        _mockBookRepository.Verify(repo => repo.AddBookAsync(It.IsAny<Book>()), Times.Once);
    }
    [Fact]
    public async Task AddBookAsync_NullBook_ThrowsArgumentException()
    {
        // Arrange — nothing to set up, we're passing null
        // Act & Assert — we expect an exception
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _bookService.AddBookAsync(null!, 1)
        );
        Assert.Equal("Book data is required", exception.Message);
        // Verify the repository was NEVER called (because it should fail before that)
        _mockBookRepository.Verify(repo => repo.AddBookAsync(It.IsAny<Book>()), Times.Never);
    }
    [Fact]
    public async Task GetBookByIdAsync_ExistingBook_ReturnsBook()
    {
        // Arrange
        var expectedBook = new Book { Id = 1, UserId = 5, Title = "Found", Author = "Author", YearPublished = 2020 };
        _mockBookRepository
            .Setup(repo => repo.GetBookByIdAsync(1, 5))
            .ReturnsAsync(expectedBook);
        // Act
        var result = await _bookService.GetBookByIdAsync(1, 5);
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Found", result.Title);
    }
    [Fact]
    public async Task GetBookByIdAsync_NonExistentBook_ThrowsKeyNotFoundException()
    {
        // Arrange — mock returns null (book not found)
        _mockBookRepository
            .Setup(repo => repo.GetBookByIdAsync(999, 5))
            .ReturnsAsync((Book?)null);
        // Act & Assert — service throws when repository returns null
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _bookService.GetBookByIdAsync(999, 5)
        );
    }
    [Fact]
    public async Task DeleteBookAsync_BookNotFound_ThrowsArgumentException()
    {
        // Arrange
        _mockBookRepository
            .Setup(repo => repo.GetBookByIdAsync(999, 1))
            .ReturnsAsync((Book?)null);
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _bookService.DeleteBookAsync(999, 1)
        );
        Assert.Equal("Book not found", exception.Message);
        // Verify delete was never called
        _mockBookRepository.Verify(repo => repo.DeleteBookAsync(It.IsAny<Book>()), Times.Never);
    }
    [Fact]
    public async Task UpdateBookAsync_ExistingBook_UpdatesFieldsCorrectly()
    {
        // Arrange
        var existingBook = new Book { Id = 1, UserId = 5, Title = "Old Title", Author = "Old Author", YearPublished = 2000 };
        var updateData = new Book { Title = "New Title", Author = "New Author", YearPublished = 2025 };
        _mockBookRepository
            .Setup(repo => repo.GetBookByIdAsync(1, 5))
            .ReturnsAsync(existingBook);
        _mockBookRepository
            .Setup(repo => repo.UpdateBookAsync(It.IsAny<Book>()))
            .ReturnsAsync((Book b) => b);
        // Act
        var result = await _bookService.UpdateBookAsync(1, updateData, 5);
        // Assert — the existing book's fields should be updated
        Assert.Equal("New Title", result.Title);
        Assert.Equal("New Author", result.Author);
        Assert.Equal(2025, result.YearPublished);
        Assert.Equal(5, result.UserId);  // UserId should NOT change
    }

    [Fact]
    public async Task UpdateBookAsync_NullBook_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _bookService.UpdateBookAsync(1, null!, 5)
        );
    }

    [Fact]
    public async Task UpdateBookAsync_BookNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockBookRepository.Setup(r => r.GetBookByIdAsync(1, 1)).ReturnsAsync((Book?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _bookService.UpdateBookAsync(1, new Book { Title = "T", Author = "A" }, 1)
        );
    }

    [Fact]
    public async Task DeleteBookAsync_Successful_CallsRepositoryDelete()
    {
        // Arrange
        var book = new Book { Id = 1, UserId = 1, Title = "Delete Me", Author = "A" };
        _mockBookRepository.Setup(r => r.GetBookByIdAsync(1, 1)).ReturnsAsync(book);
        _mockBookRepository.Setup(r => r.DeleteBookAsync(book)).Returns(Task.CompletedTask);

        // Act
        await _bookService.DeleteBookAsync(1, 1);

        // Assert
        _mockBookRepository.Verify(r => r.DeleteBookAsync(book), Times.Once);
    }
}