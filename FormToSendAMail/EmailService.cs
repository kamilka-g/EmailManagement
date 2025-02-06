using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using System.IO;
using MailKit.Security;
using FormToSendAMail.Models;

namespace FormToSendAMail.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendEmailWithTemplateAsync(string toEmail, string subject);

        Task SendCustomEmailAsync(UserInfo user);

    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Your Name", emailSettings["SmtpUsername"]));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // Ignorowanie błędów certyfikatu (jeśli problem z certyfikatem)
                client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                // Połączenie z serwerem SMTP
                await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings["SmtpUsername"], emailSettings["SmtpPassword"]);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendEmailWithTemplateAsync(string toEmail, string subject)
        {
            var htmlBody = await File.ReadAllTextAsync("emailtemplates/email.html");
            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendCustomEmailAsync(UserInfo user)
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "email.html"); //put here an path of a file which you want to send 
            var html = await File.ReadAllTextAsync("emailtemplates/email.html");

            // {} in html doc to text 
            html = html.Replace("{username}", user.Username)
                       .Replace("{message}", user.CustomMessage);

            await SendEmailAsync(user.Email, "Description of company", html);
        }
    }
}
