// <copyright file="DeeplinkController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;

namespace DeeplinkAllFeatures.Controllers
{
    /// <summary>
    /// Deeplink controller to handle multiple views. 
    /// </summary>
    public class DeeplinkController : Controller
    {
        private readonly ILogger<DeeplinkController> _logger;

        public DeeplinkController(ILogger<DeeplinkController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("DeepLinkAllFeatures")]
        public IActionResult DeepLinkAllFeatures()
        {
            return View();
        }

        [Route("Configure")]
        public IActionResult Configure()
        {
            return View();
        }
    }
}