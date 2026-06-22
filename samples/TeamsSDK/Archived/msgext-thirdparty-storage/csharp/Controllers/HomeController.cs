// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The HomeController handles navigation to various views.
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Returns the CustomForm view.
    /// </summary>
    /// <returns>An ActionResult object for the CustomForm view.</returns>
    [Route("/CustomForm")]
    public ActionResult CustomForm()
    {
        return View("CustomForm");
    }
}
