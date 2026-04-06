namespace FirstApi.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string email, string name, string subject, string body);
}