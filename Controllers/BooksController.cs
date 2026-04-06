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
                var books = await bookService.GetAllBooksAsync(userId);

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
                var book = await bookService.GetBookByIdAsync(id, userId);

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
        public async Task<ActionResult<BaseResponse<Book>>> CreateBook(Book newBook)
        {
            try
            {
                var userId = GetUserId();
                var book = await bookService.AddBookAsync(newBook, userId);
                return CreatedAtAction(nameof(GetBook), new { id = book.Id }, BaseResponse<Book>.SuccessResponse("Book created successfully", book));
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
        public async Task<ActionResult<BaseResponse<string>>> UpdateBook(int id, Book book)
        {
            try
            {
                var userId = GetUserId();
                await bookService.UpdateBookAsync(id, book, userId);
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
                await bookService.DeleteBookAsync(id, userId);
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