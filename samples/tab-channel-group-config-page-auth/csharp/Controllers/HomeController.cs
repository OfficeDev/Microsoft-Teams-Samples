using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TabAuthentication.Models;

namespace TabAuthentication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IConfiguration Configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("ConfigureTab")]
        public IActionResult ConfigureTab()
        {
            return View();
        }

        [Route("AuthStart")]
        public ActionResult AuthStart()
        {
            ViewBag.ClientId = Configuration["ClientId"].ToString();
            ViewBag.ResponseType = Configuration["ResponseType"].ToString();
            ViewBag.ResponseMode = Configuration["ResponseMode"].ToString();
            ViewBag.Resource = Configuration["Resource"].ToString();
            return View();
        }

        [Route("AuthEnd")]
        public ActionResult AuthEnd()
        {
            return View();
        }

        [Route("Success")]
        public ActionResult Success()
        {
            return View();
        }

        [Route("Failed")]
        public ActionResult Failed()
        {
            return View();
        }

        [Route("LogOut")]
        public IActionResult LogOut()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
