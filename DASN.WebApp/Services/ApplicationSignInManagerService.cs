using System.Security.Claims;
using System.Threading.Tasks;
using DASN.WebApp.Models.DataModels;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace DASN.WebApp.Services
{
    public class ApplicationSignInManagerService : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManagerService(ApplicationUserManagerService userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override async Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user) 
            => await user.GenerateUserIdentityAsync((ApplicationUserManagerService)UserManager);

        public static ApplicationSignInManagerService Create(IdentityFactoryOptions<ApplicationSignInManagerService> options, IOwinContext context) 
            => new ApplicationSignInManagerService(context.GetUserManager<ApplicationUserManagerService>(), context.Authentication);
    }
}