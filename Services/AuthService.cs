using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FirstApi.Data;
using FirstApi.Models;
using FirstApi.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FirstApi.Services;

public class AuthService
{
    private readonly FirstApiContext _context;
    private readonly IConfiguration _config;

    public AuthService(FirstApiContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<UserDto> RegisterUserAsync(RegisterRequest request)
    {
        var emailError = ValidEmail(request.Email);
        if (emailError != null)
        {
            throw new ArgumentException(emailError);
        }
        var passwordError = ValidPassword(request.Password);
        if (passwordError != null)
        {
            throw new ArgumentException(passwordError);
        }

        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new ArgumentException("User already exists");
        }


        // Create new user
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

    }

    private string? ValidEmail(string email)
    {
        if (!email.Contains("@"))
        {
            return "Email must contain @";
        }
        if (!email.Contains("."))
        {
            return "Email must contain .";
        }
        return null;
    }

    private string? ValidPassword(string password)
    {
        if (password.Length < 6)
        {
            return "Password must be at least 6 characters long";
        }
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
        // valid request body
        if (ValidEmail(request.Email) != null)
        {
            throw new ArgumentException("Invalid credentials");
        }
        if (ValidPassword(request.Password) != null)
        {
            throw new ArgumentException("Invalid credentials");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Token = token,
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            TokenExpiration = DateTime.UtcNow.AddMinutes(int.Parse(_config.GetSection("Jwt")["ExpirationInMinutes"]!))
        };

    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

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
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationInMinutes"]!)),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
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
}