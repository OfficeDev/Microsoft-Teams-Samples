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
        private readonly MsalClient _msalClient;

        public HomeController(
            IConfiguration configuration)
        {
            _configuration = configuration;
            _msalClient = new MsalClient(_configuration["AzureAd:ClientId"].ToString(), _configuration["AzureAd:TenantId"].ToString());
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.GraphData = "";
            return View();
        }

        public async Task<IActionResult> Login()
        {
            try
            {
                // Acquire the access token
                var accessToken = await _msalClient.AcquireTokenAsync();

                if (string.IsNullOrEmpty(accessToken))
                {
                    return Content("Failed to acquire access token.");
                }

                // Call the Graph API with the acquired token
                var graphData = await GraphApiClient.CallGraphApiAsync(accessToken);

                return Content(graphData);
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"An error occurred: {ex.Message}");
                return Content("An error occurred while processing your request.");
            }
        }

    }
}
