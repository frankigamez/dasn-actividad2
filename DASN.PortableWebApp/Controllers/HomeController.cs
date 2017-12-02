using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DASN.PortableWebApp.Models.DataModels;
using DASN.PortableWebApp.Models.ViewModels.Home;
using DASN.PortableWebApp.Services;
using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DASN.PortableWebApp.Controllers
{
    [SuppressMessage("ReSharper", "Mvc.ViewNotResolved")]
    public class HomeController : BaseController
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HomeController));
        
        private DASNoteService DASNoteManager { get; set; }
        
        public HomeController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            DASNoteService dasnoteManager) : base(userManager, signInManager)
        {
            DASNoteManager = dasnoteManager;
        }

        //
        // GET: /Home/Index
        public ActionResult Index() => Log.TraceActionResult(() =>
        {
            var data = new IndexViewModel();
            var user = User.Identity.IsAuthenticated
                ? CurrentUser
                : null;

            data.LastPublicPosts = DASNoteManager.GetPublicDASNotes(skip: 0, take: 10000).Select(x =>
                new PublicDASNoteViewModel
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
        });

        //
        // GET: /Home/About
        public ActionResult About() => Log.TraceActionResult(View);
        
        public ActionResult Error()=> Log.TraceActionResult(View);
    }       
}