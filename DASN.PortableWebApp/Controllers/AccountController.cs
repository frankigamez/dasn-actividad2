using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DASN.PortableWebApp.Models.DataModels;
using DASN.PortableWebApp.Models.ViewModels.Account;
using DASN.PortableWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using IdentityResult = Microsoft.AspNetCore.Identity.IdentityResult;

namespace DASN.PortableWebApp.Controllers
{
    [Authorize]
    [SuppressMessage("ReSharper", "Mvc.ViewNotResolved")]
    public class AccountController : BaseController
    {
        private static ILogger Log => Serilog.Log.Logger;
        
        private EmailSenderService EmailSenderService { get; set; }

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            EmailSenderService emailSenderService) : base(userManager, signInManager)
        {
            EmailSenderService = emailSenderService;
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
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
            => await new AntibotService(seconds: 5).SecureActionAsync(async () =>
            {
                if (!ModelState.IsValid)
                    return View(model);

                // Require the user to have a confirmed email before they can log on.
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user != null && !await UserManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "You must have a confirmed email to log in.");
                    return View(model);
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                var result = await SignInManager.PasswordSignInAsync(userName: model.Email,
                    password: model.Password,
                    isPersistent: model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                    return Url.IsLocalUrl(returnUrl)
                        ? (ActionResult) Redirect(returnUrl)
                        : RedirectToAction("Index", "Home");

                if (result.IsLockedOut)
                    return View("Lockout");

                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            });

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register() => View();
            

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) 
                return View(model);

            var user = new ApplicationUser {UserName = model.Email, Email = model.Email};
            var result = await UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                //return RedirectToAction("Index", "Home");

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new {code = code, token = user.Id},
                    protocol: HttpContext.Request.Scheme);
                try
                {
                    await EmailSenderService.SendEmailAsync(email: model.Email, subject: "Confirm your account",
                        message: "Please confirm your account by clicking here: " + callbackUrl);
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

            var user = await UserManager.FindByIdAsync(token);
            var result = await UserManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword() => View();

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {            
            if (!ModelState.IsValid) 
                return View(model);

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null || !await UserManager.IsEmailConfirmedAsync(user))
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");

            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
            // Send an email with this link
            var code = await UserManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new {code = code},
                protocol: HttpContext.Request.Scheme);
            try
            {
                await EmailSenderService.SendEmailAsync(email: user.Email, subject: "Reset Password",
                    message: "Please reset your password by clicking here: " + callbackUrl);
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
        public ActionResult ForgotPasswordConfirmation() => View();

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code) => code == null ? View("Error") : View();

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model) => await new AntibotService(5).SecureActionAsync(async () =>
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");

            var result = await UserManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation", "Account");

            AddErrors(result);
            return View();
        });

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation() => View();

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff()
        {
            await SignInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing) base.Dispose(false);
            EmailSenderService = null;
            base.Dispose(true);
        }
       
        private void AddErrors(IdentityResult result)
            => result.Errors.ToList().ForEach(x => ModelState.AddModelError("", x.Description));        
    }
}