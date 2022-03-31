using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppInMeeting.Controllers
{
    public class HomeController : Controller
    {
        [Route("appInMeeting")]
        public IActionResult AppInMeeting()
        {
            return View();
        }

        [Route("taskInfo")]
        public IActionResult TaskInfo()
        {
            return View();
        }

        [Route("todo")]
        public IActionResult ToDo()
        {
            return PartialView();
        }

        [Route("done")]
        public IActionResult Doing()
        {
            return PartialView();
        }

        [Route("doing")]
        public IActionResult Done()
        {
            return PartialView();
        }
    }
}
