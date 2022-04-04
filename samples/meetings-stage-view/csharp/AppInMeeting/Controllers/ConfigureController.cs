using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppInMeeting.Controllers
{
    public class ConfigureController : Controller
    {
        [Route("configure")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
