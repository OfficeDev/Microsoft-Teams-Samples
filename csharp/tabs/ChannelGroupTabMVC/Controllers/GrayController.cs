using Microsoft.AspNetCore.Mvc;
using ChannelGroupTabMVC.Models;

namespace ChannelGroupTabMVC.Controllers
{
    [Route("gray")]
    public class GrayController : Controller
    {
        [Route("")]
        public IActionResult Gray()
        {
            ChannelGroup status = new ChannelGroup();
            ViewBag.Message = status.GetStatus();
            return View();
        }
    }
}