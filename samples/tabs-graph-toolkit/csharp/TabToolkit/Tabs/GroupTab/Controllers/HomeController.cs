using Microsoft.AspNetCore.Mvc;

namespace Tabs
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Config()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TermsOfUse()
        {
            return View();
        }
    }
}