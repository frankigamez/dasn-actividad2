using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DASN.PortableWebApp.Services
{
    public class EmailSenderServiceSettings
    {
        public string From { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
                
    }
    
    public class EmailSenderService
    {
        private readonly IOptions<EmailSenderServiceSettings> _settings;
        
        public EmailSenderService(IOptions<EmailSenderServiceSettings> serviceSettings)
        {
            _settings = serviceSettings;
        }
                                        
        public async Task SendEmailAsync(string email, string subject, string message)
        {            
            var mail = new MimeMessage
            {
                Subject = subject,
                Body = new TextPart("plain") {Text = message},
            };
            mail.From.Add(new MailboxAddress("DASN", _settings.Value.From));
            mail.To.Add(new MailboxAddress(email, email));            

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_settings.Value.Host, _settings.Value.Port, _settings.Value.EnableSsl);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                await client.AuthenticateAsync(_settings.Value.From, _settings.Value.Password);
                await client.SendAsync(mail);
                await client.DisconnectAsync(true);
            }
        }
    }
}