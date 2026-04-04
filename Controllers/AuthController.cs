using FirstApi.DTOs;
using FirstApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<BaseResponse<UserDto>>> RegisterUser(RegisterRequest request)
    {
        try
        {
            var user = await authService.RegisterUserAsync(request);
            
            return Ok(BaseResponse<UserDto>.SuccessResponse("User registered successfully", user));
        }
        catch (ArgumentException e)
        {
            return BadRequest(BaseResponse<UserDto>.ErrorResponse(e.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, BaseResponse<UserDto>.ErrorResponse("An error occurred while registering the user"));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<BaseResponse<AuthResponse>>> LoginUser(LoginRequest request)
    {
        try
        {

            var response = await authService.LoginUserAsync(request);
            return Ok(BaseResponse<AuthResponse>.SuccessResponse("User logged in successfully", response));
        }
        catch (UnauthorizedAccessException e)
        {
            return Unauthorized(BaseResponse<AuthResponse>.ErrorResponse(e.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, BaseResponse<AuthResponse>.ErrorResponse("An error occurred while logging in the user"));
        }
    }
}