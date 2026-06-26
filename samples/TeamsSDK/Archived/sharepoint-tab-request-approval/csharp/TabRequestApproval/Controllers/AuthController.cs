// <copyright file="AuthController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabRequestApproval.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Sample app auth controller.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration instance.</param>
        public AuthController(
            IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Authentication start.
        /// </summary>
        /// <returns>current View.</returns>
        public IActionResult Start()
        {
            this.ViewBag.AzureClientId = this.configuration["AzureAd:MicrosoftAppId"];
            return this.View();
        }

        /// <summary>
        /// Authentication end.
        /// </summary>
        /// <returns>current View.</returns>
        public IActionResult End()
        {
            this.ViewBag.AzureClientId = this.configuration["AzureAd:MicrosoftAppId"];
            return this.View();
        }
    }
}