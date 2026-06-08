// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MeetingLiveCaption.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Returns view to be displayed in Task Module.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Returns view to be displayed for meeting tab configuration page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Configure()
        {
            return View();
        }
    }
}
