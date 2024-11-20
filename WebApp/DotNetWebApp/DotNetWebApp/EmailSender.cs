using Microsoft.AspNetCore.Identity.UI.Services; // To import IEmailSender
using Microsoft.Extensions.Options; // To import IOptions
using System.Net.Mail;
using System.Net;


namespace DotNetWebApp
{
    public class EmailSender : IEmailSender
    {
        private readonly IOptions<EmailSettings> _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpClient = new SmtpClient(_emailSettings.Value.Host)
            {
                Port = _emailSettings.Value.Port,
                Credentials = new NetworkCredential(_emailSettings.Value.Username, _emailSettings.Value.Password),
                EnableSsl = true,  // Upewnij się, że SSL/TLS jest włączone
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.Value.FromAddress, _emailSettings.Value.FromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                // Złap i zaloguj szczegóły wyjątku
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;  // Rzuć ponownie, aby błąd był propagowany
            }
        }
    }

}
