// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

namespace TeamsAuthSSO.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Threading.Tasks;
     

    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewBag.clientId = _configuration["AzureAd:ClientId"].ToString();
            return View();
        }
    }
}
