// Copyright(c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;

namespace TabChannelGroupQuickstart
{
    /// <summary>
    /// HomeController is responsible for handling routes related to the main pages of the application.
    /// This includes rendering the main index page, hello page, and other specific views.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Displays the home page (Index view).
        /// </summary>
        /// <returns>Returns the Index view.</returns>
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays the Tab page.
        /// </summary>
        /// <returns>Returns the Tab view.</returns>
        [Route("tab")]
        public IActionResult Tab()
        {
            return View();
        }

        /// <summary>
        /// Displays the Tab Configuration page.
        /// </summary>
        /// <returns>Returns the TabConfig view.</returns>
        [Route("config")]
        public IActionResult TabConfig()
        {
            return View("TabConfig");
        }              
    }
}