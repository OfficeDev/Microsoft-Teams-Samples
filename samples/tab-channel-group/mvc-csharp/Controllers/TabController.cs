using Microsoft.AspNetCore.Mvc;
using ChannelGroupTabMVC.Models;


namespace ChannelGroupTabMVC.Controllers
{
    [Route("tab")]
    public class TabController : Controller
    {
        [Route("")]
        public IActionResult Tab()
        {
            ChannelGroup color = new ChannelGroup();
            ViewBag.Gray = color.GetGray();
            ViewBag.Red = color.GetRed();
            return View();
        }
    }
}