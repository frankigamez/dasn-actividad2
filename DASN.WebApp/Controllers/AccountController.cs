using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DASN.WebApp.Models.DataModels;
using DASN.WebApp.Models.ViewModels.Account;
using DASN.WebApp.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace DASN.WebApp.Controllers
{
    [Authorize]
    [HandleError]
    public class AccountController : Controller
    {
        private ApplicationSignInManagerService _signInManager;
        private ApplicationUserManagerService _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManagerService userManager,
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
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
            => new AntibotService(seconds: 5).SecureAction(() =>
            {
                if (!ModelState.IsValid)
                    return View(model);

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                var result = SignInManager.PasswordSignIn(model.Email, model.Password, model.RememberMe,
                    shouldLockout: false);
                switch (result)
                {
                    case SignInStatus.Success:
                        return Url.IsLocalUrl(returnUrl)
                            ? (ActionResult)Redirect(returnUrl)
                            : RedirectToAction("Index", "Home");
                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                }
            });

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
            => View();

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser {UserName = model.Email, Email = model.Email};
            var result = await UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                //return RedirectToAction("Index", "Home");

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { code = code, token = user.Id}, protocol: Request.Url.Scheme);
                try
                {
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account",
                        "Please confirm your account by clicking here: " + callbackUrl);
                    return View("ConfirmRegister");
                }
                catch (Exception e)
                {
                    return Redirect(callbackUrl);
                }                
            }
            AddErrors(result);

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string code, string token)
        {
            if (code == null || token == null)
                return View("Error");

            var result = await UserManager.ConfirmEmailAsync(token, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
            => View();

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");

            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
            // Send an email with this link
            var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { code = code}, protocol: Request.Url.Scheme);
            try
            {
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking here: " + callbackUrl);
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }
            catch (Exception e)
            {
                return Redirect(callbackUrl);
            }            
            // If we got this far, something failed, redisplay form
            //return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
            => View();

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
            => code == null ? View("Error") : View();

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
            => new AntibotService(5).SecureAction(() =>
            {
                if (!ModelState.IsValid)
                    return View(model);

                var user = UserManager.FindByEmail(model.Email);
                if (user == null)
                    // Don't reveal that the user does not exist
                    return RedirectToAction("ResetPasswordConfirmation", "Account");

                var result = UserManager.ResetPassword(user.Id, model.Code, model.Password);
                if (result.Succeeded)
                    return RedirectToAction("ResetPasswordConfirmation", "Account");

                AddErrors(result);
                return View();
            });

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
            => View();

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }        

        protected override void Dispose(bool disposing)
        {
            if (!disposing) base.Dispose(false);
            _userManager?.Dispose();
            _userManager = null;
            _signInManager?.Dispose();
            _signInManager = null;
            base.Dispose(true);
        }
       
        private void AddErrors(IdentityResult result)
            => result.Errors.ToList().ForEach(x => ModelState.AddModelError("", x));        
    }
}