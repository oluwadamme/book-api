using Microsoft.AspNetCore.Mvc;
using FirstApi.Models;
using FirstApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FirstApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly FirstApiContext _context;

        public BooksController(FirstApiContext context)
        {
            _context = context;
        }


        // static private List<Book> _books =
        // [
        //     new Book { Id = 1, Title = "Book 1", Author = "Author 1", YearPublished = 2022 },
        //     new Book { Id = 2, Title = "Book 2", Author = "Author 2", YearPublished = 2023 },
        //     new Book { Id = 3, Title = "Book 3", Author = "Author 3", YearPublished = 2024 },
        //     new Book { Id = 4, Title = "Book 4", Author = "Author 4", YearPublished = 2025 },
        //     new Book { Id = 5, Title = "Book 5", Author = "Author 5", YearPublished = 2026 },
        //     new Book { Id = 6, Title = "Book 6", Author = "Author 6", YearPublished = 2027 },
        //     new Book { Id = 7, Title = "Book 7", Author = "Author 7", YearPublished = 2028 },
        //     new Book { Id = 8, Title = "Book 8", Author = "Author 8", YearPublished = 2029 },
        //     new Book { Id = 9, Title = "Book 9", Author = "Author 9", YearPublished = 2030 },
        //     new Book { Id = 10, Title = "Book 10", Author = "Author 10", YearPublished = 2031 }

        // ];

        [HttpGet]
        public async Task<ActionResult<List<Book>>> GetBooks()
        {
           var books = await _context.Books.ToListAsync();
             

            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
                var books = await _context.Books.ToListAsync();
            var book = books.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult<Book>> CreateBook(Book? newBook)
        {
        var books = _context.Books;
            if (newBook == null)
            {
                return BadRequest();
            }

            if (books.Any(b => b.Id == newBook.Id))
            {
                return BadRequest("Book with this id already exists");
            }

            newBook.Id = books.Max(b => b.Id) + 1;
            _context.Books.Add(newBook);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBook), new { id = newBook.Id }, newBook);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, Book? book)
        {   
            if (book == null)
            {
                return BadRequest();
            }
            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (existingBook == null)
            {
                return NotFound();
            }
            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.YearPublished = book.YearPublished;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}