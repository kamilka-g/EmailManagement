using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using System.IO;
using MailKit.Security;
using FormToSendAMail.Models;
using Microsoft.Extensions.Logging;


namespace FormToSendAMail.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendEmailWithTemplateAsync(string toEmail, string subject);

        Task SendCustomEmailAsync(User user);

    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private async Task SendEmailWithRetryAsync(MimeMessage emailMessage, int maxRetries = 3, int delayBetweenRetriesMs = 2000)
        {
            int retryCount = 0;
            bool emailSent = false;

            while (retryCount < maxRetries && !emailSent)
            {
                try
                {
                    using (var client = new SmtpClient())
                    {
                        client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                        await client.ConnectAsync(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:SmtpPort"]), SecureSocketOptions.StartTls);
                        await client.AuthenticateAsync(_configuration["EmailSettings:SmtpUsername"], _configuration["EmailSettings:SmtpPassword"]);
                        await client.SendAsync(emailMessage);
                        await client.DisconnectAsync(true);
                        emailSent = true;

                        _logger.LogInformation("E-mail wysłany pomyślnie.");
                    }
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogError($"Błąd przy wysyłaniu e-maila. Próba {retryCount} z {maxRetries}. Szczegóły: {ex.Message}");

                    if (retryCount < maxRetries)
                    {
                        _logger.LogInformation($"Opóźnienie przed kolejną próbą: {delayBetweenRetriesMs}ms");
                        await Task.Delay(delayBetweenRetriesMs); // Czekamy przed ponowną próbą
                    }
                    else
                    {
                        _logger.LogError("Nie udało się wysłać e-maila po kilku próbach.");
                        throw;
                    }
                }
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Kamila-mail-management ", emailSettings["SmtpUsername"]));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            emailMessage.Body = bodyBuilder.ToMessageBody();

            // Wywołujemy metodę z retry
            await SendEmailWithRetryAsync(emailMessage);
        }

        public async Task SendEmailWithTemplateAsync(string toEmail, string subject)
        {
            var htmlBody = await File.ReadAllTextAsync("emailtemplates/email.html");
            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendCustomEmailAsync(User user)
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "email.html");
            var html = await File.ReadAllTextAsync("emailtemplates/email.html");

            html = html.Replace("{username}", user.Username)
                       .Replace("{message}", user.CustomMessage)
                       .Replace("{firstName}", user.FirstName);

            await SendEmailAsync(user.Email, "Kamila Company Name", html);
        }
    }
}
