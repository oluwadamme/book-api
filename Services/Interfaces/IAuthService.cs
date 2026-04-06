using FirstApi.DTOs;
namespace FirstApi.Services.Interfaces;

public interface IAuthService
{
    Task<UserDto> RegisterUserAsync(RegisterRequest request);
    Task<AuthResponse> LoginUserAsync(LoginRequest request);
    Task<bool> VerifyEmailAsync(VerifyEmailRequest request);
    Task<EmailVerificationStatus> ResendEmailVerificationTokenAsync(ForgetPasswordRequest request);
    Task<bool> ForgotPasswordAsync(ForgetPasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
}
public enum EmailVerificationStatus
{
    Verified,
    NotVerified,
    UserNotFound
}