using Microsoft.AspNetCore.Mvc;

namespace PersonalTabMVC.Controllers
{
    [Route("privacy")]
    public class PrivacyController : Controller
    {
        [Route("")]
        public IActionResult Privacy()
        {
            ViewBag.Message = "Add your privacy statement here...";
            return View();
        }
    }
}