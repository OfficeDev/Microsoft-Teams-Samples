using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringLocalizer<HomeController> _localizer;

        public HomeController(IStringLocalizer<HomeController> localizer)
        {
            _localizer = localizer;
        }

        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        

        [Route("hello")]
        public ActionResult Hello()
        {
            return View("Index");
        }


        [Route("configure")]
        public ActionResult Configure()
        {
            return View();
        }
    }
}