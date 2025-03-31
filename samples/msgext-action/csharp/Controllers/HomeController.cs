using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamsMessagingExtensionsAction.Controllers
{
    /// <summary>
    /// HomeController handles the routing for various views.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Returns the RazorView view.
        /// </summary>
        [Route("/Home/RazorView")]
        public IActionResult RazorView()
        {
            return View("RazorView");
        }

        /// <summary>
        /// Returns the CustomForm view.
        /// </summary>
        [Route("/Home/CustomForm")]
        public IActionResult CustomForm()
        {
            return View("CustomForm");
        }

        /// <summary>
        /// Returns the HtmlPage view.
        /// </summary>
        [Route("/Home/HtmlPage")]
        public IActionResult HtmlPage()
        {
            return View("HtmlPage");
        }
    }
}
