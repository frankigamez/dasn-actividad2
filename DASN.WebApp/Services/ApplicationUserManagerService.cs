using System;
using DASN.WebApp.Models;
using DASN.WebApp.Models.DataModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace DASN.WebApp.Services
{
    public class ApplicationUserManagerService : UserManager<ApplicationUser>
    {
        public ApplicationUserManagerService(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManagerService Create(IdentityFactoryOptions<ApplicationUserManagerService> options, IOwinContext context)
        {
            var manager = new ApplicationUserManagerService(new UserStore<ApplicationUser>(context.Get<DASNDbContext>()));

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;
            
            // Configure email for notifications
            manager.EmailService = new EmailService();
           
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("DASN Identity"));
            return manager;
        }
    }
}