using DASN.PortableWebApp.Models.DataModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DASN.PortableWebApp.Controllers
{
    public class BaseController : Controller
    {
        protected BaseController(Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager,
            Microsoft.AspNetCore.Identity.SignInManager<ApplicationUser> siginManager)
        {
            UserManager = userManager;
            SignInManager = siginManager;
        }

        protected Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> UserManager { get; }
        
        protected Microsoft.AspNetCore.Identity.SignInManager<ApplicationUser> SignInManager { get; }

        protected ApplicationUser CurrentUser => UserManager.GetUserAsync(HttpContext.User).Result;
        protected string CurrentUserId => HttpContext.User.Identity.GetUserId();
        protected string CurrentUserName => HttpContext.User.Identity.GetUserName();
        
        protected override void Dispose(bool disposing)
        {
            if (!disposing) base.Dispose(false);
            UserManager?.Dispose();
            base.Dispose(true);
        }
    }
}