using System.Web.Mvc;

namespace Infra.Identity.Server.Areas.v1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}