// <copyright file="AuthController.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace TabAuthEntraAccount.Controllers
{
    /// <summary>
    /// AuthController manages the authentication flow for Microsoft Entra ID (formerly Azure AD).
    /// This controller provides the authentication start and end pages that handle the OAuth 2.0
    /// authorization code flow within the Microsoft Teams tab application context.
    /// </summary>
    public class AuthController : Controller
    {
        // Configuration service for accessing Azure AD settings from appsettings.json
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// Sets up dependency injection for configuration access.
        /// </summary>
        /// <param name="configuration">IConfiguration instance for accessing app settings</param>
        public AuthController(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Displays the authentication start page for Microsoft Entra ID OAuth 2.0 flow.
        /// This page initiates the authentication process by redirecting users to the Microsoft login page.
        /// The Azure AD Client ID is passed to the view to configure the JavaScript authentication flow.
        /// </summary>
        /// <returns>AuthStart view with Azure AD Client ID configured for authentication</returns>
        public IActionResult AuthStart()
        {
            // Provide the Azure AD Client ID to the view for JavaScript authentication initialization
            ViewBag.AzureClientId = _configuration["AzureAd:ClientId"];
            return View();
        }

        /// <summary>
        /// Displays the authentication completion page for Microsoft Entra ID OAuth 2.0 flow.
        /// This page is shown after the user completes authentication with Microsoft and 
        /// the authorization code is returned. It typically handles the code exchange and 
        /// user profile retrieval process.
        /// </summary>
        /// <returns>AuthEnd view for handling authentication completion</returns>
        public IActionResult AuthEnd()
        {
            return View();
        }
    }
}