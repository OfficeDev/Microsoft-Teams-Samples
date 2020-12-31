using Microsoft.AspNetCore.Mvc;

namespace PersonalTabMVC
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Message = "It Works!";
            return View();
        }
    }
}