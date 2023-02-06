using Microsoft.AspNetCore.Mvc;

namespace TabPeoplePicker.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// This enpoint is called to load initial page for tab people picker app.
        /// </summary>
        [Route("index")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// This enpoint is called to load configuration page for tab people picker app.
        /// </summary>
        [Route("configure")]
        [HttpGet]
        public IActionResult ConfigureTab()
        {
            return View();
        }
    }
}