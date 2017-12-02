using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DASN.PortableWebApp.Models.DataModels;
using DASN.PortableWebApp.Models.ViewModels.Manage;
using DASN.PortableWebApp.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DASN.PortableWebApp.Controllers
{
    [Authorize]
    [SuppressMessage("ReSharper", "Mvc.ViewNotResolved")]
    public class ManageController : BaseController
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ManageController));
        
        public ManageController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) : base(userManager, signInManager)
        {
        }

        //
        // GET: /Manage/Index
        public ActionResult Index(ManageMessageId? message) => Log.TraceActionResult(() =>
        {
            Log.TraceEntry(name: nameof(message), value: message);
            
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
        });

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
            => Log.TraceActionResult(View);

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model) => Log.TraceActionResult(() =>
        {
            Log.TraceEntry(name: nameof(model), value: model);
            
            if (!ModelState.IsValid)
                return View(model);

            var result = UserManager.ChangePasswordAsync(CurrentUser, model.OldPassword,
                model.NewPassword).Result;
            if (result.Succeeded)
            {
                var user = CurrentUser;
                if (user != null)
                    SignInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", new {Message = ManageMessageId.ChangePasswordSuccess});
            }

            AddErrors(result);
            return View(model);
        });

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        
        private void AddErrors(IdentityResult result)
            => result.Errors.ToList().ForEach(error => ModelState.AddModelError("", error.Description));
        
        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemovePhoneSuccess,
            Error
        }
    }
}