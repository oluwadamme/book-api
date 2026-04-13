using FirstApi.DTOs;
using FirstApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [EnableRateLimiting("AuthLimit")]
    [HttpPost("register")]
    public async Task<ActionResult<BaseResponse<UserDto>>> RegisterUser(RegisterRequest request)
    {
        var user = await authService.RegisterUserAsync(request);

        return CreatedAtAction(nameof(RegisterUser), new { id = user.Id }, BaseResponse<UserDto>.SuccessResponse("User registered successfully", user));
    }
    [EnableRateLimiting("AuthLimit")]
    [HttpPost("login")]
    public async Task<ActionResult<BaseResponse<AuthResponse>>> LoginUser(LoginRequest request)
    {
        var response = await authService.LoginUserAsync(request);
            return Ok(BaseResponse<AuthResponse>.SuccessResponse("User logged in successfully", response));
    }
    [EnableRateLimiting("AuthLimit")]
    [HttpPost("verify-email")]
    public async Task<ActionResult<BaseResponse<bool>>> VerifyEmail(VerifyEmailRequest request)
    {
        var result = await authService.VerifyEmailAsync(request);
        return Ok(BaseResponse<bool>.SuccessResponse("Email verified successfully", result));
    }
    [EnableRateLimiting("AuthLimit")]
    [HttpPost("resend-email-verification-token")]
    public async Task<ActionResult<BaseResponse<bool>>> ResendEmailVerificationToken(ForgetPasswordRequest request)
    {
        var result = await authService.ResendEmailVerificationTokenAsync(request);

        return Ok(BaseResponse<bool>.SuccessResponse("Email verification token resent successfully", result));

    }
    [EnableRateLimiting("AuthLimit")]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<BaseResponse<bool>>> ForgotPassword(ForgetPasswordRequest request)
    {
        var result = await authService.ForgotPasswordAsync(request);
        return Ok(BaseResponse<bool>.SuccessResponse("Password reset token sent successfully", result));
    }
    [EnableRateLimiting("AuthLimit")]
    [HttpPost("reset-password")]
    public async Task<ActionResult<BaseResponse<bool>>> ResetPassword(ResetPasswordRequest request)
    {
        var result = await authService.ResetPasswordAsync(request);
        return Ok(BaseResponse<bool>.SuccessResponse("Password reset successfully", result));
    }
    [EnableRateLimiting("AuthLimit")]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<BaseResponse<AuthResponse>>> RefreshToken(RefreshTokenRequest request)
    {
        var response = await authService.RefreshTokenAsync(request);
        return Ok(BaseResponse<AuthResponse>.SuccessResponse("Token refreshed successfully", response));
    }
    [EnableRateLimiting("AuthLimit")]
    [HttpPost("logout")]
    public async Task<ActionResult<BaseResponse<bool>>> Logout(RefreshTokenRequest request)
    {
        var response = await authService.RevokeRefreshTokenAsync(request);
        return Ok(BaseResponse<bool>.SuccessResponse("Logged out successfully", response));
    }
}