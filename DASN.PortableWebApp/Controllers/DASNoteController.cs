using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DASN.PortableWebApp.Models.DataModels;
using DASN.PortableWebApp.Models.ViewModels.DASNote;
using DASN.PortableWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DASN.PortableWebApp.Controllers
{
    [SuppressMessage("ReSharper", "Mvc.ViewNotResolved")]
    public class DASNoteController : BaseController
    {
        private static ILogger Log => Serilog.Log.Logger;
        
        private DASNoteService DASNoteManager { get; set; }
        
        public DASNoteController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            DASNoteService dasnoteManager) : base(userManager, signInManager)
        {
            DASNoteManager = dasnoteManager;
        }
      
        //
        // GET: /DASNote/Index
        [HttpGet]
        public ActionResult Index() => RedirectToAction(controllerName: "Home", actionName: "Index");

        //
        // GET: /DASNote/Create
        [HttpGet]
        [Authorize]
        public ActionResult Create() => View();

        //
        // POST: /DASNote/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateDASNoteViewModel model) 
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = CurrentUser;

            DASNoteManager.AddDASNote(
                content: model.Content,
                isPublic: model.IsPublic,
                createdAt: DateTime.UtcNow,
                userId: user.Id);

            return View("ConfirmCreate");
        }

        //
        // GET: /DASNote/AuthoredBy/{id}
        [HttpGet]
        public async Task<ActionResult> AuthoredBy(string id)
        {
            var guid = Guid.Parse(id);

            var user = await UserManager.FindByIdAsync(guid.ToString());

            var model = new ShowDASNoteCollectionViewModel
            {
                CreatedBy = user.UserName,
                CreatorToken = user.Id
            };
            model.AddRange(DASNoteManager.GetPublicDASNotesByUser(user: user, skip: 0, take: 1000)
                .Select(x => new ShowDASNoteViewModel
                {
                    Content = x.Content,
                    CreatedAt = x.CreatedAt,
                    IsPublic = x.IsPublic,
                    CreatedBy = x.User.UserName,
                    CreatorToken = x.User.Id,
                    DASNoteToken = x.Id.ToString()
                }));

            return View("AuthoredBy", (ShowDASNoteCollectionViewModel) model);
        }


        //
        // GET: /DASNote/MyPosts
        [HttpGet]
        [Authorize]
        public ActionResult MyDASNotes()
        {
            var user = CurrentUser;

            var model = new ShowDASNoteCollectionViewModel
            {
                CreatedBy = user.UserName,
                CreatorToken = user.Id
            };
            model.AddRange(DASNoteManager.GetDASNotesByUser(user: user, skip: 0, take: 1000)
                .Select(x => new ShowDASNoteViewModel
                {
                    Content = x.Content,
                    CreatedAt = x.CreatedAt,
                    IsPublic = x.IsPublic,
                    CreatedBy = x.User.UserName,
                    CreatorToken = x.User.Id,
                    DASNoteToken = x.Id.ToString()
                }));

            return View("AuthoredBy", model);
        }


        //
        // GET: /DASNote/Show/{id}
        [HttpGet]
        public ActionResult Show(string id)
        {            
            var data = DASNoteManager.GetDASNoteById(Guid.Parse(id));

            ViewBag.User = CurrentUserId;

            return View(new ShowDASNoteViewModel
            {
                CreatedAt = data.CreatedAt,
                Content = data.Content,
                CreatedBy = data.User.UserName,
                CreatorToken = data.User.Id,
                IsPublic = data.IsPublic,
                DASNoteToken = data.Id.ToString()
            });
        }

        //
        // GET: /DASNote/Delete/{id}
        [HttpGet]
        [Authorize]
        public ActionResult Delete(string id)
        {            
            var data = DASNoteManager.GetDASNoteById(Guid.Parse(id));
            var user = CurrentUser;

            if (data.UserId != user.Id)
                return RedirectToAction(controllerName: "Account", actionName: "Login");

            ViewBag.UrlReferrer = HttpContext.Request.Headers["Referer"];

            return View(new ShowDASNoteViewModel
            {
                CreatedAt = data.CreatedAt,
                Content = data.Content,
                CreatedBy = data.User.UserName,
                CreatorToken = data.User.Id,
                IsPublic = data.IsPublic,
                DASNoteToken = data.Id.ToString()
            });
        }

        //
        // POST: /DASNote/ConfirmDelete
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDelete(string dasNoteToken)
        {
            var data = DASNoteManager.GetDASNoteById(Guid.Parse(dasNoteToken));
            var user = CurrentUser;

            if (data.UserId != user.Id)
                return RedirectToAction(controllerName: "Account", actionName: "Login");

            DASNoteManager.RemoveDASNote(
                id: data.Id,
                user: user);

            return View("ConfirmDelete");
        }
    }
}