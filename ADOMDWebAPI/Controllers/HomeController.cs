using System.Web.Mvc;

namespace ADOMDWebAPI.Controllers
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
