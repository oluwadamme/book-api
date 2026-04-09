using System.ComponentModel.DataAnnotations;
namespace FirstApi.DTOs;

public class ResetPasswordRequest
{
    [Required, EmailAddress] public string Email { get; set; }
    [Required, MinLength(6)] public string Password { get; set; }
    [Required] public string Token { get; set; }
}