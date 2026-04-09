using FirstApi.Models;

namespace FirstApi.Repositories.Interfaces;

public interface IBookRepository
{
    Task<Book?> GetBookByIdAsync(int id, int userId);
    Task<List<Book>> GetAllBooksAsync(int userId);
    Task<Book> AddBookAsync(Book book);
    Task<Book> UpdateBookAsync(Book book);
    Task DeleteBookAsync(Book book);
}
