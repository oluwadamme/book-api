using MailKit.Net.Smtp;
using MimeKit;

namespace FirstApi.Services;

public class EmailService(IConfiguration config, ILogger<EmailService> logger)
{
    public async Task SendEmailAsync(string email, string name, string subject, string body)
    {
        try
        {
        var emailSettings = config.GetSection("EmailSettings");
        var smtpServer = emailSettings["SmtpServer"];
        var smtpPort = int.Parse(emailSettings["SmtpPort"]!);
        var senderEmail = emailSettings["SenderEmail"]!;
        var senderName = emailSettings["SenderName"];
        var password = emailSettings["Password"]!;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress(name, email));
        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = $@"Hey {name},

            {body}

            -- {senderName}"
        };
        using var client = new SmtpClient();
        await client.ConnectAsync(smtpServer, smtpPort, false);

        // Note: only needed if the SMTP server requires authentication
        await client.AuthenticateAsync(senderEmail, password);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
        }
        catch (SmtpProtocolException)
        {
            throw;
        }
        catch (SmtpCommandException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending the email");
            throw new Exception($"Failed to send email: {ex.Message}");
        }
    }
}