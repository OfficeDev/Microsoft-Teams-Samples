using Microsoft.AspNetCore.Mvc;

namespace PersonalTabMVC.Controllers
{
    [Route("tou")]
    public class TouController : Controller
    {
        [Route("")]
        public IActionResult Tou()
        {
            ViewBag.Message = "Add your Terms of Use statement here...";
            return View();
        }
    }
}