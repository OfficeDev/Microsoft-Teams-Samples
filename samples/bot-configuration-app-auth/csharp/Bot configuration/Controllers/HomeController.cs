// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;

namespace Bot_configuration.Controllers
{
    /// <summary>
    /// HomeController serves the home page for the bot web application
    /// </summary>
    public class HomeController : Microsoft.AspNetCore.Mvc.Controller
    {
        [Route("")]
        [Route("Home")]
        [Route("Home/Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}