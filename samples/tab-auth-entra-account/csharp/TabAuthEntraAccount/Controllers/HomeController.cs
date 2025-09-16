// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

namespace TabAuthEntraAccount.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Models;
    using System.Diagnostics;

    /// <summary>
    /// HomeController handles the main application functionality for the Microsoft Teams tab application.
    /// With the PKCE (Proof Key for Code Exchange) flow implementation, this controller now only serves
    /// the initial UI and configuration, while all authentication and Microsoft Graph API calls
    /// are handled client-side in JavaScript.
    /// </summary>
    public class HomeController : Controller
    {
        // Configuration service for accessing appsettings.json values
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Main entry point for the Teams tab application.
        /// Sets up the Azure AD Client ID for use in the JavaScript authentication flow.
        /// </summary>
        /// <returns>The Index view with Azure AD Client ID configured in ViewBag</returns>
        public IActionResult Index()
        {
            // Pass the Azure AD Client ID to the view for JavaScript authentication
            ViewBag.AzureClientId = _configuration["AzureAd:ClientId"];
            return View();
        }

        /// <summary>
        /// Handles application errors and displays the error page.
        /// This action is called when an unhandled exception occurs in the application.
        /// It provides debugging information in development environments.
        /// </summary>
        /// <returns>Error view with diagnostic information</returns>
        // Disable response caching for error pages to ensure fresh error information
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Create error view model with current request/activity ID for correlation
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}