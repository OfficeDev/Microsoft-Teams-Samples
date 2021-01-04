using Microsoft.AspNetCore.Mvc;


namespace ChannelGroupTabMVC
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
