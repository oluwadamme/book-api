using FirstApi.DTOs;
using FirstApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService, ILogger<AuthController> logger) : ControllerBase
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
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while registering the user");
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
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while logging in the user");
            return StatusCode(500, BaseResponse<AuthResponse>.ErrorResponse("An error occurred while logging in the user"));
        }
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<BaseResponse<bool>>> VerifyEmail(VerifyEmailRequest request)
    {
        try
        {

            var result = await authService.VerifyEmailAsync(request);
            if (!result)
            {
                return BadRequest(BaseResponse<bool>.ErrorResponse("Invalid or expired verification token"));
            }
            return Ok(BaseResponse<bool>.SuccessResponse("Email verified successfully", true));
        }
        catch (ArgumentException e)
        {
            return BadRequest(BaseResponse<bool>.ErrorResponse(e.Message));
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while verifying the email");
            return StatusCode(500, BaseResponse<bool>.ErrorResponse("An error occurred while verifying the email"));
        }
    }

    [HttpPost("resend-email-verification-token")]
    public async Task<ActionResult<BaseResponse<bool>>> ResendEmailVerificationToken(ForgetPasswordRequest request)
    {
        try
        {
            var result = await authService.ResendEmailVerificationTokenAsync(request);

            if (result == AuthService.EmailVerificationStatus.Verified)
            {
                return BadRequest(BaseResponse<bool>.ErrorResponse("Email already verified"));
            }
            return Ok(BaseResponse<bool>.SuccessResponse("Email verification token resent successfully", true));
        }
        catch (ArgumentException e)
        {
            return BadRequest(BaseResponse<bool>.ErrorResponse(e.Message));
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while resending the email verification token");
            return StatusCode(500, BaseResponse<bool>.ErrorResponse("An error occurred while resending the email verification token"));
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<BaseResponse<bool>>> ForgotPassword(ForgetPasswordRequest request)
    {
        try
        {
            await authService.ForgotPasswordAsync(request);
            return Ok(BaseResponse<bool>.SuccessResponse("Password reset token sent successfully", true));
        }
        catch (ArgumentException e)
        {
            return BadRequest(BaseResponse<bool>.ErrorResponse(e.Message));
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while sending the password reset token");
            return StatusCode(500, BaseResponse<bool>.ErrorResponse("An error occurred while sending the password reset token"));
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<BaseResponse<bool>>> ResetPassword(ResetPasswordRequest request)
    {
        try
        {
            var result = await authService.ResetPasswordAsync(request);
            if (!result)
            {
                return BadRequest(BaseResponse<bool>.ErrorResponse("Invalid or expired verification token"));
            }
            return Ok(BaseResponse<bool>.SuccessResponse("Password reset successfully", true));
        }
        catch (ArgumentException e)
        {
            return BadRequest(BaseResponse<bool>.ErrorResponse(e.Message));
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while resetting the password");
            return StatusCode(500, BaseResponse<bool>.ErrorResponse("An error occurred while resetting the password"));
        }
    }
}