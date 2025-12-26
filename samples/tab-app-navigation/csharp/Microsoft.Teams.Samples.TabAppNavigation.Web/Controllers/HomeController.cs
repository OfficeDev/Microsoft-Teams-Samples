// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Microsoft.AspNetCore.Mvc provides attributes and classes for building web APIs and MVC web apps in ASP.NET Core.
using Microsoft.AspNetCore.Mvc;

namespace TabAppNavigation
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
        /// Displays the default tab view.
        /// </summary>
        /// <returns>Returns the default tab view.</returns>
        [Route("default_tab")]
        public ActionResult defaultTab()
        {
            return View("TabAppNavigation");
        }


        /// <summary>
        /// Displays Page1.
        /// </summary>
        /// <returns>Returns the Page1 view.</returns>
        [Route("page1")]
        public IActionResult Page1()
        {
            return View();
        }

        /// <summary>
        /// Displays Page2.
        /// </summary>
        /// <returns>Returns the Page2 view.</returns>
        [Route("page2")]
        public IActionResult Page2()
        {
            return View();
        }

        /// <summary>
        /// Displays Page3.
        /// </summary>
        /// <returns>Returns the Page3 view.</returns>
        [Route("page3")]
        public IActionResult Page3()
        {
            return View();
        }

        /// <summary>
        /// Displays the TabConfig page.
        /// </summary>
        /// <returns>Returns the TabConfig view.</returns>
        [Route("configure")]
        public IActionResult TabConfig()
        {
            return View("TabConfig");
        }
    }
}