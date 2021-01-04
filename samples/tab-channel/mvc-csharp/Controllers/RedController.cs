using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ChannelGroupTabMVC.Models;

namespace ChannelGroupTabMVC.Controllers
{
    [Route("red")]
    public class RedController : Controller
    {
        [Route("")]
        public IActionResult Red()
        {
            ChannelGroup status = new ChannelGroup();
            ViewBag.Message = status.GetStatus();
            return View();
        }
    }
}