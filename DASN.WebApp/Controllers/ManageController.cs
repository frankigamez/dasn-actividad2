using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DASN.WebApp.Models.ViewModels.Manage;
using DASN.WebApp.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace DASN.WebApp.Controllers
{
    [Authorize]
    [HandleError]
    public class ManageController : Controller
    {
        private ApplicationSignInManagerService _signInManager;
        private ApplicationUserManagerService _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManagerService userManager,
            ApplicationSignInManagerService signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManagerService SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManagerService>();
            private set => _signInManager = value;
        }

        public ApplicationUserManagerService UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManagerService>();
            private set => _userManager = value;
        }

        //
        // GET: /Manage/Index
        public ActionResult Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess
                    ? "Your password has been changed."
                    : message == ManageMessageId.SetPasswordSuccess
                        ? "Your password has been set."
                        : message == ManageMessageId.SetTwoFactorSuccess
                            ? "Your two-factor authentication provider has been set."
                            : message == ManageMessageId.Error
                                ? "An error has occurred."
                                : message == ManageMessageId.AddPhoneSuccess
                                    ? "Your phone number was added."
                                    : message == ManageMessageId.RemovePhoneSuccess
                                        ? "Your phone number was removed."
                                        : "";
            return View();
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
            => View();

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                    SignInManager.SignIn(user, isPersistent: false, rememberBrowser: false);

                return RedirectToAction("Index", new {Message = ManageMessageId.ChangePasswordSuccess});
            }

            AddErrors(result);
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        
        private void AddErrors(IdentityResult result)
            => result.Errors.ToList().ForEach(error => ModelState.AddModelError("", error));
        
        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }
    }
}