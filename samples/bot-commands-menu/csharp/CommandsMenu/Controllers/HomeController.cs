// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    // HomeController is responsible for handling requests to the root of the web application.
    // It contains the Index action that returns the default view.
    public class HomeController : Controller
    {
        // The Index method handles requests to the root URL ("/").
        // It returns the default view for the home page of the application.
        [Route("")]
        public ActionResult Index()
        {
            return View(); // Returns the default view for the home page
        }
    }
}
