using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ChannelGroupTabMVC.Controllers
{
    [Route("privacy")]
    public class PrivacyController : Controller
    {
        [Route("")]
        public IActionResult Privacy()
        {
            ViewBag.Message = "Add your privacy statement here...";
            return View();
        }
    }
}