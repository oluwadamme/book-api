using System.ComponentModel.DataAnnotations;
namespace FirstApi.DTOs;

public class RegisterRequest
{
    [Required, MinLength(2)] public string FirstName { get; set; }
    [Required, MinLength(2)] public string LastName { get; set; }
    [Required, EmailAddress] public string Email { get; set; }
    [Required, MinLength(6)] public string Password { get; set; }
}