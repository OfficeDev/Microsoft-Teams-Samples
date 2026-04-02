using System;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    // HomeController handles the default page request.
    public class HomeController : Controller
    {
        // The Index action returns the default view when accessing the root URL.
        public IActionResult Index()
        {
            return View();
        }
    }
}
