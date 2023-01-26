using Microsoft.AspNetCore.Mvc;

namespace DeeplinkAllFeatures.Controllers
{
    public class DeeplinkController : Controller
    {
        private readonly ILogger<DeeplinkController> _logger;

        public DeeplinkController(ILogger<DeeplinkController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("DeepLinkAllFeatures")]
        public IActionResult DeepLinkAllFeatures()
        {
            return View();
        }

        [Route("Configure")]
        public IActionResult Configure()
        {
            return View();
        }
    }
}