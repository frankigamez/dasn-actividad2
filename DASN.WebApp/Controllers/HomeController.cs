using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DASN.WebApp.Models;
using DASN.WebApp.Models.ViewModels.Home;
using DASN.WebApp.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace DASN.WebApp.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        private DASNoteService _dasnoteManager;
        private ApplicationUserManagerService _userManager;

        public HomeController()
        {
        }

        public HomeController(ApplicationUserManagerService userManager, DASNoteService postManager)
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
        // GET: /Home/Index
        public ActionResult Index()
        {
            var data = new IndexViewModel();
            var user = User.Identity.IsAuthenticated
                ? UserManager.FindById(User?.Identity?.GetUserId())
                : null;

            data.LastPublicPosts = DASNoteManager.GetPublicDASNotes(skip: 0, take: 10000).Select(x => new PublicDASNoteViewModel
            {
                CreatedAt = x.CreatedAt,
                CreatedBy = x.User.UserName,
                Content = x.Content,
                CreatorToken = Guid.Parse(x.User.Id),
                DASNoteToken = x.Id                    
            }).ToList();

            data.MyLastsPosts = user == null
                ? new List<MyDASNoteViewModel>()
                : DASNoteManager.GetDASNotesByUser(user: user, skip: 0, take: 3).Select(x => new MyDASNoteViewModel
                {
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.User.UserName,
                    Content = x.Content,
                    IsPublic = x.IsPublic,
                    CreatorToken = Guid.Parse(x.User.Id),
                    DASNoteToken = x.Id
                }).ToList();

            data.IsAuthed = User.Identity.IsAuthenticated;

            return View(data);
        }

        //
        // GET: /Home/About
        public ActionResult About() 
            => View();
    }
}