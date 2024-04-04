using Bookify.Web.Core.Settings;
using Humanizer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Bookify.Web.Helpers.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public EmailSender(IOptions<EmailSettings> settings, IWebHostEnvironment webHostEnvironment)
        {
            _settings = settings.Value;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using SmtpClient smtpClient = new(_settings.Host, _settings.Port);

            smtpClient.Credentials = new NetworkCredential(_settings.Email, _settings.Password);
            smtpClient.EnableSsl = true;

            using MailMessage mail = new()
            {
                From = new MailAddress(_settings.Email, _settings.DisplayName),
                Subject = subject,
                IsBodyHtml = true,
                Body = htmlMessage
            };

            mail.To.Add(_webHostEnvironment.IsDevelopment() ? "ziadelkappo@gmail.com" : email);

            await smtpClient.SendMailAsync(mail);
        }
    }
}
