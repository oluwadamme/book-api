using System.ComponentModel.DataAnnotations;
namespace FirstApi.DTOs;

public class ForgetPasswordRequest
{
    [Required, EmailAddress] public string Email { get; set; }
}