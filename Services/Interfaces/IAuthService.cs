using FirstApi.DTOs;
namespace FirstApi.Services.Interfaces;

public interface IAuthService
{
    Task<UserDto> RegisterUserAsync(RegisterRequest request);
    Task<AuthResponse> LoginUserAsync(LoginRequest request);
    Task<bool> VerifyEmailAsync(VerifyEmailRequest request);
    Task<bool> ResendEmailVerificationTokenAsync(ForgetPasswordRequest request);
    Task<bool> ForgotPasswordAsync(ForgetPasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> RevokeRefreshTokenAsync(RefreshTokenRequest request);
}
