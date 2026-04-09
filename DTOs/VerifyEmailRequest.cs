namespace FirstApi.DTOs;

public class VerifyEmailRequest
{
    public string? Email { get; set; }
    public string? Token { get; set; }
}