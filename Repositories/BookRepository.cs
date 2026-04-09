using FirstApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using FirstApi.Data;
using FirstApi.Models;

namespace FirstApi.Repositories;

public class BookRepository(FirstApiContext context) : IBookRepository
{
    public async Task<Book?> GetBookByIdAsync(int id, int userId)
    {
        return await context.Books.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
    }

    public async Task<List<Book>> GetAllBooksAsync(int userId)
    {
        return await context.Books.AsNoTracking().Where(b => b.UserId == userId).OrderByDescending(b => b.Id).ToListAsync();
    }

    public async Task<Book> AddBookAsync(Book book)
    {
        await context.Books.AddAsync(book);
        await context.SaveChangesAsync();
        return book;
    }

    public async Task<Book> UpdateBookAsync(Book book)
    {
        context.Books.Update(book);
        await context.SaveChangesAsync();
        return book;
    }

    public async Task DeleteBookAsync(Book book)
    {
        context.Books.Remove(book);
        await context.SaveChangesAsync();
    }
}