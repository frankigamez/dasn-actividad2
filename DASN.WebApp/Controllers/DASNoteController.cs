using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DASN.WebApp.Models;
using DASN.WebApp.Models.ViewModels.DASNote;
using DASN.WebApp.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace DASN.WebApp.Controllers
{
    [HandleError]
    public class DASNoteController : Controller
    {
        private DASNoteService _dasnoteManager;
        private ApplicationUserManagerService _userManager;

        public DASNoteController()
        {
        }

        public DASNoteController(ApplicationUserManagerService userManager, DASNoteService postManager)
        {
            UserManager = userManager;
            DASNoteManager = _dasnoteManager;
        }

        public DASNoteService DASNoteManager
        {
            get => _dasnoteManager ?? new DASNoteService(HttpContext.GetOwinContext().Get<DASNDbContext>());
            private set => _dasnoteManager = value;
        }

        public ApplicationUserManagerService UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManagerService>();
            private set => _userManager = value;
        }

        //
        // GET: /DASNote/Index
        [HttpGet]
        public ActionResult Index() => RedirectToAction(controllerName: "Home", actionName: "Index");

        //
        // GET: /DASNote/Create
        [HttpGet]
        [Authorize]
        public ActionResult Create() 
            => View();


        //
        // POST: /DASNote/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateDASNoteViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = UserManager.FindById(User.Identity.GetUserId());

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
        public ActionResult AuthoredBy(string id)
        {
            var guid = Guid.Parse(id);

            var user = UserManager.FindById(guid.ToString());
            
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

            return View("AuthoredBy", (ShowDASNoteCollectionViewModel)model);
        }


        //
        // GET: /DASNote/MyPosts
        [HttpGet]
        [Authorize]
        public ActionResult MyDASNotes()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            
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
            var user = UserManager.FindById(User.Identity.GetUserId());

            if (data.UserId != user.Id)
                return RedirectToAction(controllerName: "Account", actionName: "Login");
            
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
        public ActionResult ConfirmDelete(string DASNoteToken)
        {
            var data = DASNoteManager.GetDASNoteById(Guid.Parse(DASNoteToken));
            var user = UserManager.FindById(User.Identity.GetUserId());

            if (data.UserId != user.Id)
                return RedirectToAction(controllerName: "Account", actionName: "Login");

            DASNoteManager.RemoveDASNote(
                id: data.Id,
                user: user);

            return View("ConfirmDelete");
        }
    }
}