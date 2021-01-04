using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ChannelGroupTabMVC.Controllers
{
    [Route("tou")]
    public class TouController : Controller
    {
        [Route("")]
        public IActionResult Tou()
        {
            ViewBag.Message = "Add your Terms of Use statement here...";
            return View();
        }
    }
}