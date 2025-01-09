using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamsMessagingExtensionsAction.Controllers
{
    public class HomeController : Controller
    {
        [Route("/CustomForm")]
        public ActionResult CustomForm()
        {
            return View("CustomForm");
        }
    }
}
