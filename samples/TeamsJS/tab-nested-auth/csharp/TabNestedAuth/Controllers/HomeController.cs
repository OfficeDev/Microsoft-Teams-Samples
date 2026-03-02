// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

// Declare the namespace for the controller
namespace TabNestedAuth.Controllers
{
    // Import necessary namespaces
    using Microsoft.AspNetCore.Mvc;  // Provides functionalities for building web applications, including controllers.
    using Microsoft.Extensions.Configuration;  // Provides access to configuration settings.

    // Define the HomeController class, which inherits from the ASP.NET Core Controller class
    public class HomeController : Controller
    {
        // Private readonly field to store the injected IConfiguration instance
        private readonly IConfiguration _configuration;

        // Constructor that accepts an IConfiguration instance as a dependency and assigns it to the private field
        public HomeController(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Action method that handles HTTP GET requests to the Index endpoint
        public IActionResult Index()
        {
            // Set the clientId value in the ViewBag for use in the view
            ViewBag.clientId = _configuration["AzureAd:ClientId"].ToString();
            return View();  // Return the default view associated with this action
        }
    }
}
