using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FirstApi.Models;
using FirstApi.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using FirstApi.Options;
using Microsoft.Extensions.Options;
using FirstApi.Services.Interfaces;
using FirstApi.Repositories.Interfaces;
using Hangfire;
namespace FirstApi.Services;

public class AuthService(IAuthRepository authRepository, IEmailService emailService, ILogger<AuthService> logger, IOptions<EmailVerificationOptions> emailOptions, IOptions<JwtOptions> jwtOptions) : IAuthService
{
    public async Task<UserDto> RegisterUserAsync(RegisterRequest request)
    {
        var passwordError = ValidatePassword(request.Password);
        if (passwordError != null)
        {
            throw new ArgumentException(passwordError);
        }

        // Check if user already exists
        if (await authRepository.ExistsByEmailAsync(request.Email))
        {
            throw new ArgumentException("User already exists");
        }

        var emailVerificationToken = GenerateVerificationToken();
        var subject = "Verify your email";
        var body = $"Thanks for registering! Please verify your email by using the code below: {emailVerificationToken}";

        // Create new user
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsEmailVerified = false,
            EmailVerificationToken = emailVerificationToken,
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(emailOptions.Value.ExpirationInMinutes)
        };

        await authRepository.AddUserAsync(user);
        BackgroundJob.Enqueue<IEmailService>(x =>
        x.SendEmailAsync(user.Email, user.FirstName, subject, body));

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            IsEmailVerified = user.IsEmailVerified
        };

    }

    public async Task<bool> ResendEmailVerificationTokenAsync(ForgetPasswordRequest request)
    {
        var email = request.Email;
        var user = await authRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        if (user.IsEmailVerified)
        {
            throw new ArgumentException("User is already verified");
        }
        var emailVerificationToken = GenerateVerificationToken();
        var subject = "Verify your email";
        var body = $"Thanks for registering! Please verify your email by using the code below: {emailVerificationToken}";

        user.EmailVerificationToken = emailVerificationToken;
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(emailOptions.Value.ExpirationInMinutes);
        await authRepository.UpdateUserAsync(user);
        BackgroundJob.Enqueue<IEmailService>(x =>
        x.SendEmailAsync(user.Email, user.FirstName, subject, body));
        return true;
    }


    public async Task<bool> ForgotPasswordAsync(ForgetPasswordRequest request)
    {
        var email = request.Email;
        var user = await authRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            return false;
        }
        var expirationInMinutes = emailOptions.Value.ExpirationInMinutes;
        var passwordResetToken = GenerateVerificationToken();
        var subject = "Reset your password — FirstApi";
        var body = $"Your password reset code is: {passwordResetToken}\nThis code expires in {expirationInMinutes} minutes.\nIf you didn't request this, you can ignore this email.";

        user.PasswordResetToken = passwordResetToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(expirationInMinutes);
        await authRepository.UpdateUserAsync(user);
        BackgroundJob.Enqueue<IEmailService>(x =>
        x.SendEmailAsync(user.Email, user.FirstName, subject, body));
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var passwordError = ValidatePassword(request.Password);
        if (passwordError != null)
        {
            throw new ArgumentException(passwordError);
        }
        var user = await authRepository.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        if (user.PasswordResetToken != request.Token || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid token");
        }
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await authRepository.UpdateUserAsync(user);
        return true;
    }

    private string GenerateVerificationToken()
    {
        // generate 4 digit otp, if it is development env, the code will be 0000 else it will generate random code
        var token = RandomNumberGenerator.GetInt32(10000).ToString("D4");
        return token;
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var user = await authRepository.GetUserByEmailAndTokenAsync(request.Email, request.Token);
        if (user == null || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid token");
        }
        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        await authRepository.UpdateUserAsync(user);
        return true;
    }



    private string? ValidatePassword(string password)
    {
        if (!password.Any(char.IsUpper))
        {
            return "Password must contain at least one uppercase letter";
        }
        if (!password.Any(char.IsLower))
        {
            return "Password must contain at least one lowercase letter";
        }
        if (!password.Any(char.IsDigit))
        {
            return "Password must contain at least one digit";
        }
        if (password.All(char.IsLetterOrDigit))
        {
            return "Password must contain at least one special character";
        }
        return null;
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }


    public async Task<AuthResponse> LoginUserAsync(LoginRequest request)
    {
        var user = await authRepository.GetUserByEmailAsync(request.Email);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // if (!user.IsEmailVerified)
        // {
        //     throw new ArgumentException("Email not verified. Please verify your email to login.");
        // }

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationInDays);
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = refreshTokenExpiry;
        await authRepository.UpdateUserAsync(user);

        return new AuthResponse
        {
            Token = token,
            Id = user.Id,
            RefreshToken = refreshToken,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            TokenExpiration = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationInMinutes),
            IsEmailVerified = user.IsEmailVerified
        };

    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var oldRefreshToken = request.RefreshToken;
        if (oldRefreshToken == null)
        {
            throw new UnauthorizedAccessException("Refresh token is required");
        }
        var user = await authRepository.GetUserByRefreshTokenAsync(oldRefreshToken);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }
        var token = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationInDays);
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = refreshTokenExpiry;
        await authRepository.UpdateUserAsync(user);
        return new AuthResponse
        {
            Token = token,
            Id = user.Id,
            RefreshToken = newRefreshToken,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            TokenExpiration = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationInMinutes),
            IsEmailVerified = user.IsEmailVerified
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwt = jwtOptions.Value;
        var key = Encoding.UTF8.GetBytes(jwt.Key);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(jwt.ExpirationInMinutes),
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
            TokenType = "Bearer"
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<bool> RevokeRefreshTokenAsync(RefreshTokenRequest request)
    {
        var oldRefreshToken = request.RefreshToken;
        if (oldRefreshToken == null)
        {
            throw new UnauthorizedAccessException("Refresh token is required");
        }
        var user = await authRepository.GetUserByRefreshTokenAsync(oldRefreshToken);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await authRepository.UpdateUserAsync(user);
            return true;
        }
        return false;
    }

}