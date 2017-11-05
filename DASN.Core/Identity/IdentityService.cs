using System.Collections.Generic;
using System.Security.Claims;
using DASN.Core.Models.DataModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace DASN.Core.Identity
{
    public class IdentityService
    {
        private readonly UserManager<ApplicationUser, string> _userManager;
        private IAuthenticationManager _authManager;
        private readonly SignInManager<ApplicationUser, string> _signinManager;

        public IdentityService(IUserStore<ApplicationUser, string> store, IAuthenticationManager authManager)
        {
            _userManager = new UserManager<ApplicationUser, string>(store);
            _authManager = authManager;
            _signinManager = new SignInManager<ApplicationUser, string>(_userManager, authManager);
        }

        public void LoginWithRegister(ApplicationUser user, bool persistent = true, bool remember = false)
        {           
            _signinManager.SignIn(user: user, isPersistent: persistent, rememberBrowser: remember);
        }
        public void LoginWithCredentials(string email, string password, bool persistent = true, bool lockout = false)
        {
            _signinManager.PasswordSignIn(userName: email, password: password, isPersistent: persistent, shouldLockout: lockout);
        }


        public void Logout(ApplicationUser user, bool persistent = true, bool remember = false)
        {           
            
        }

        public ClaimsIdentity GenerateUserIdentity(ApplicationUser user) =>
            _userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
    }
}