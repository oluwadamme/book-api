
namespace FirstApi.DTOs;

public class AuthResponse
{
    public string Token { get; set; }
    public string? RefreshToken { get; set; }
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; } 

    public DateTime TokenExpiration { get; set; }
}