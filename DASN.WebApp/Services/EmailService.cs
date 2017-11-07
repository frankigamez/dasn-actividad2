using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace DASN.WebApp.Services
{
    public class EmailService : IIdentityMessageService
    {        
        public Task SendAsync(IdentityMessage message)
        {
            var client = new SmtpClient();
            return client.SendMailAsync("fgm00026.dasn@gmail.com",
                message.Destination,
                message.Subject,
                message.Body);
        }
    }
}