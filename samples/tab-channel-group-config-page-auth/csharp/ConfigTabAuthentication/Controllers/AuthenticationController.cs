using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TabAuthentication.Models;

namespace TabAuthentication.Controllers
{
    public class AuthenticationController : Controller
    {

        private readonly ILogger<AuthenticationController> _logger;

        private readonly IConfiguration Configuration;

        public AuthenticationController(ILogger<AuthenticationController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }
        public IActionResult Index()
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
    }
}
