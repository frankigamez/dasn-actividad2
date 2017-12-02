using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DASN.PortableWebApp.Models.DataModels;
using DASN.PortableWebApp.Models.ViewModels.Account;
using DASN.PortableWebApp.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IdentityResult = Microsoft.AspNetCore.Identity.IdentityResult;

namespace DASN.PortableWebApp.Controllers
{
    [Authorize]
    [SuppressMessage("ReSharper", "Mvc.ViewNotResolved")]
    public class AccountController : BaseController
    {                
        private static readonly ILog Log = LogManager.GetLogger(typeof(AccountController));
        
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
        public ActionResult Login(string returnUrl) => Log.TraceActionResult(() =>
        {
            Log.TraceEntry(name: nameof(returnUrl), value: returnUrl);
            ViewBag.ReturnUrl = returnUrl;
            return View();
        });

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl) => Log.TraceActionResult(()
            => new AntibotService(seconds: 5).SecureActionAsync(async () =>
            {
                Log.TraceEntry(name: nameof(model), value: model);
                Log.TraceEntry(name: nameof(returnUrl), value: returnUrl);                
                
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
            }).Result);

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
            => Log.TraceActionResult(View);

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model) => Log.TraceActionResult(() =>
        {
            Log.TraceEntry(name: nameof(model), value: model);
            
            if (!ModelState.IsValid) 
                return View(model);

            var user = new ApplicationUser {UserName = model.Email, Email = model.Email};
            var result = UserManager.CreateAsync(user, model.Password).Result;
            if (result.Succeeded)
            {
                //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                //return RedirectToAction("Index", "Home");

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                var code = UserManager.GenerateEmailConfirmationTokenAsync(user).Result;
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new {code = code, token = user.Id},
                    protocol: HttpContext.Request.Scheme);
                try
                {
                    EmailSenderService.SendEmail(email: model.Email, subject: "Confirm your account",
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
        });

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string code, string token) => Log.TraceActionResult(() =>
        {
            Log.TraceEntry(name: nameof(code), value: code);
            Log.TraceEntry(name: nameof(token), value: token);
            
            if (code == null || token == null)
                return View("Error");

            var user = UserManager.FindByIdAsync(token).Result;
            var result = UserManager.ConfirmEmailAsync(user, code).Result;
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        });

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
            => Log.TraceActionResult(View);

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model) => Log.TraceActionResult(() =>
        {
            Log.TraceEntry(name: nameof(model), value: model);
            
            if (!ModelState.IsValid) 
                return View(model);

            var user = UserManager.FindByEmailAsync(model.Email).Result;
            if (user == null || !UserManager.IsEmailConfirmedAsync(user).Result)
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");

            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
            // Send an email with this link
            var code = UserManager.GeneratePasswordResetTokenAsync(user).Result;
            var callbackUrl = Url.Action("ResetPassword", "Account", new {code = code},
                protocol: HttpContext.Request.Scheme);
            try
            {
                EmailSenderService.SendEmail(email: user.Email, subject: "Reset Password",
                    message: "Please reset your password by clicking here: " + callbackUrl);
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }
            catch (Exception e)
            {
                return Redirect(callbackUrl);
            }
            // If we got this far, something failed, redisplay form
            //return View(model);
        });

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
            => Log.TraceActionResult(View);

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
            => Log.TraceActionResult( () => code == null ? View("Error") : View());

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model) => Log.TraceActionResult(() =>
        {
            return new AntibotService(5).SecureActionAsync(async () =>
            {
                Log.TraceEntry(name: nameof(model), value: model);
                
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
            }).Result;
        });

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
            => Log.TraceActionResult(View);

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff() => Log.TraceActionResult(() =>
        {
            SignInManager.SignOutAsync().GetAwaiter().GetResult();
            return RedirectToAction("Index", "Home");
        });

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