using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using FirstApi.Models;
using Microsoft.AspNetCore.Authorization;
using FirstApi.DTOs;
using FirstApi.Services.Interfaces;
namespace FirstApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BooksController(IBookService bookService) : ControllerBase
    {
        // Helper method to get the logged-in user's ID from the JWT token
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }
            return int.Parse(userIdClaim);
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponse<List<Book>>>> GetBooks()
        {
            var userId = GetUserId();
            var books = await bookService.GetAllBooksAsync(userId);

            return Ok(BaseResponse<List<Book>>.SuccessResponse("Books fetched successfully", books));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponse<Book>>> GetBook(int id)
        {
            var userId = GetUserId();
            var book = await bookService.GetBookByIdAsync(id, userId);

            return Ok(BaseResponse<Book>.SuccessResponse("Book fetched successfully", book));

        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<Book>>> CreateBook(CreateBookRequest request)
        {
            var userId = GetUserId();
            var newBook = new Book
            {
                Title = request.Title,
                Author = request.Author,
                YearPublished = request.YearPublished,

            };
            var book = await bookService.AddBookAsync(newBook, userId);
            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, BaseResponse<Book>.SuccessResponse("Book created successfully", book));

        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponse<string>>> UpdateBook(int id, CreateBookRequest request)
        {
            var userId = GetUserId();
            var book = new Book
            {
                Title = request.Title,
                Author = request.Author,
                YearPublished = request.YearPublished,

            };
            await bookService.UpdateBookAsync(id, book, userId);
            return Ok(BaseResponse<string>.SuccessResponse("Book updated successfully", "Book updated successfully"));

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponse<string>>> DeleteBook(int id)
        {
            var userId = GetUserId();
            await bookService.DeleteBookAsync(id, userId);
            return NoContent();

        }
    }
}