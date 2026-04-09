using System.ComponentModel.DataAnnotations;
namespace FirstApi.DTOs;

public class VerifyEmailRequest
{
    [Required, EmailAddress] public string Email { get; set; }
    [Required] public string Token { get; set; }
}