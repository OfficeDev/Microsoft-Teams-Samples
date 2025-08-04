﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace TabAuthEntraAccount.Controllers
{
    public class AuthController : Controller
    {

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        public AuthController(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Authentication Start page for Google
        /// </summary>
        /// <returns>current View</returns>
        public IActionResult GoogleStart()
        {
            ViewBag.AzureClientId = _configuration["AzureAd:ClientId"];
            return View();
        }

        /// <summary>
        /// Authentication End page for Google
        /// </summary>
        /// <returns>current View</returns>
        public IActionResult GoogleEnd()
        {
            return View();
        }
    }
}