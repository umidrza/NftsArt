using System.Net.Mail;
using System.Net;
using NftsArt.Model.Helpers;

namespace NftsArt.BL.Services;


public interface IEmailService
{
    Task<Result<string>> SendEmailAsync(string email, string subject, string message);
}

public class EmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;

    public EmailService()
    {
        _smtpClient = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            Credentials = new NetworkCredential("your-email", "your-password")
        };
    }

    public async Task<Result<string>> SendEmailAsync(string email, string subject, string message)
    {
        try
        {
            var mailMessage = new MailMessage("your-email", email, subject, message);
            await _smtpClient.SendMailAsync(mailMessage);
            return Result<string>.Success(email, "Email has been sent successfully!");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.ToString());
        }
    }
}
