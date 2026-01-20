// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Used for configuring and hosting ASP.NET Core web applications.
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace TabAppNavigation
{
    /// The entry point for the application, responsible for configuring and starting the web host.
    public class Program
    {
        /// <summary>
        /// Main method to configure and start the web application.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application.</param>
        public static void Main(string[] args)
        {
            // Build and run the web host
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Configures the web host builder with the required settings.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application.</param>
        /// <returns>The configured IHostBuilder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Specify the Startup class to be used for configuration
                    webBuilder.UseStartup<Startup>();
                });
    }
}
