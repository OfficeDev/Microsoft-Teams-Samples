// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;

namespace CommandsMenu.Controllers;

public class HomeController : Controller
{
    [Route("")]
    public ActionResult Index()
    {
        return View();
    }
}
