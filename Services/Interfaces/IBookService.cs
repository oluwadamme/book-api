using FirstApi.Models;

namespace FirstApi.Services.Interfaces;

public interface IBookService
{
    Task<Book?> GetBookByIdAsync(int id, int userId);
    Task<List<Book>> GetAllBooksAsync(int userId);
    Task<Book> AddBookAsync(Book book, int userId);
    Task<Book> UpdateBookAsync(int id, Book book, int userId);
    Task DeleteBookAsync(int id, int userId);
}