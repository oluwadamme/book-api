using FirstApi.Repositories.Interfaces;
using FirstApi.Services.Interfaces;
using FirstApi.Models;
namespace FirstApi.Services;

public class BookService(IBookRepository bookRepository) : IBookService
{
    public async Task<Book?> GetBookByIdAsync(int id, int userId)
    {
        return await bookRepository.GetBookByIdAsync(id, userId);
    }

    public async Task<List<Book>> GetAllBooksAsync(int userId)
    {
        return await bookRepository.GetAllBooksAsync(userId);
    }

    public async Task<Book> AddBookAsync(Book book, int userId)
    {
        if (book == null)
        {
            throw new ArgumentException("Book data is required");
        }
        book.UserId = userId;
        var newBook = await bookRepository.AddBookAsync(book);
        return newBook;
    }

    public async Task<Book> UpdateBookAsync(int id, Book book, int userId)
    {
        if (book == null)
        {
            throw new ArgumentException("Book data is required");
        }
        var existingBook = await bookRepository.GetBookByIdAsync(id, userId);
        if (existingBook == null)
        {
            throw new ArgumentException("Book not found");
        }
        existingBook.Title = book.Title;
        existingBook.Author = book.Author;
        existingBook.YearPublished = book.YearPublished;
        var updatedBook = await bookRepository.UpdateBookAsync(existingBook);
        return updatedBook;
    }

    public async Task DeleteBookAsync(int id, int userId)
    {
        var existingBook = await bookRepository.GetBookByIdAsync(id, userId);
        if (existingBook == null)
        {
            throw new ArgumentException("Book not found");
        }
        await bookRepository.DeleteBookAsync(existingBook);
    }

    
}