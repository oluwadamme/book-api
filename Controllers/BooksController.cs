using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using FirstApi.Models;
using FirstApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FirstApi.DTOs;
namespace FirstApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BooksController(FirstApiContext context) : ControllerBase
    {
        // Helper method to get the logged-in user's ID from the JWT token
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new ArgumentException("User not found");
            }
            return int.Parse(userIdClaim);
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponse<List<Book>>>> GetBooks()
        {
            try
            {
                var userId = GetUserId();
                var books = await context.Books
                    .Where(b => b.UserId == userId)
                    .ToListAsync();

                return Ok(BaseResponse<List<Book>>.SuccessResponse("Books fetched successfully", books));
            }
            catch (ArgumentException e)
            {
                return Unauthorized(BaseResponse<Book>.ErrorResponse(e.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, BaseResponse<Book>.ErrorResponse("An error occurred while fetching the books"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponse<Book>>> GetBook(int id)
        {
            try
            {
                var userId = GetUserId();
                var book = await context.Books
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                if (book == null)
                {
                    return NotFound(BaseResponse<Book>.ErrorResponse("Book data is required"));
                }

                return Ok(BaseResponse<Book>.SuccessResponse("Book fetched successfully", book));
            }
            catch (ArgumentException e)
            {
                return Unauthorized(BaseResponse<Book>.ErrorResponse(e.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, BaseResponse<Book>.ErrorResponse("An error occurred while fetching the books"));
            }

        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<Book>>> CreateBook(Book? newBook)
        {
            try
            {
                if (newBook == null)
                {
                    return BadRequest(BaseResponse<Book>.ErrorResponse("Book data is required"));
                }

                newBook.UserId = GetUserId();
                context.Books.Add(newBook);
                await context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetBook), new { id = newBook.Id }, BaseResponse<Book>.SuccessResponse("Book created successfully", newBook));
            }
            catch (ArgumentException e)
            {
                return Unauthorized(BaseResponse<Book>.ErrorResponse(e.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, BaseResponse<Book>.ErrorResponse("An error occurred while fetching the books"));
            }

        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponse<string>>> UpdateBook(int id, Book? book)
        {
            try
            {
                if (book == null)
                {
                    return BadRequest(BaseResponse<string>.ErrorResponse("Book data is required"));
                }

                var userId = GetUserId();
                var existingBook = await context.Books
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                if (existingBook == null)
                {
                    return NotFound(BaseResponse<string>.ErrorResponse("Book not found"));
                }

                existingBook.Title = book.Title;
                existingBook.Author = book.Author;
                existingBook.YearPublished = book.YearPublished;
                await context.SaveChangesAsync();
                return Ok(BaseResponse<string>.SuccessResponse("Book updated successfully", "Book updated successfully"));
            }
            catch (ArgumentException e)
            {
                return Unauthorized(BaseResponse<string>.ErrorResponse(e.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, BaseResponse<string>.ErrorResponse("An error occurred while fetching the books"));
            }

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponse<string>>> DeleteBook(int id)
        {
            try
            {
                var userId = GetUserId();
                var book = await context.Books
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

                if (book == null)
                {
                    return NotFound(BaseResponse<string>.ErrorResponse("Book not found"));
                }

                context.Books.Remove(book);
                await context.SaveChangesAsync();
                return Ok(BaseResponse<string>.SuccessResponse("Book deleted successfully", "Book deleted successfully"));
            }
            catch (ArgumentException e)
            {
                return Unauthorized(BaseResponse<string>.ErrorResponse(e.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, BaseResponse<string>.ErrorResponse("An error occurred while fetching the books"));
            }
        }
    }
}