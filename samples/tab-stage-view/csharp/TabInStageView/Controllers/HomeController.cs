namespace TabInStageView.Controllers
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    public class HomeController : Controller
    {
        private readonly string _appId;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configuration">configuration of application.</param>
        public HomeController(IConfiguration configuration)
        {
            _appId = configuration["MicrosoftAppId"] ?? throw new NullReferenceException("MicrosoftAppId");
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("sampleTab")]
        public ActionResult SampleTab()
        {
            // Setting client id to be used in deep link in tab
            ViewBag.AppId = _appId;
            return View();
        }
    }
}
