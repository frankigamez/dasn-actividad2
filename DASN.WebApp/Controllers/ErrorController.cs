using System.Web.Mvc;

namespace DASN.WebApp.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
            => View("Error");
    }
}