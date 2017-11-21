using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace DASN.PortableWebApp.Services
{
    public class EmailSenderService
    {
        private const string From = "fgm00026.dasn@gmail.com";
        private const string Host = "smtp.gmail.com";
        private const string Password = "fgm00026.123!";
        private const int Port = 587;
        private const bool EnableSSL = false;
                                        
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mail = new MimeMessage
            {
                Subject = subject,
                Body = new TextPart("plain") {Text = message},
            };
            mail.From.Add(new MailboxAddress("DASN", From));
            mail.To.Add(new MailboxAddress(email, email));            

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(Host, Port, EnableSSL);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                await client.AuthenticateAsync(From, Password);
                await client.SendAsync(mail);
                await client.DisconnectAsync(true);
            }
        }
    }
}