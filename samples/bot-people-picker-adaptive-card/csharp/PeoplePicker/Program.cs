// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.14.0

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace PeoplePicker
{
    /// <summary>
    /// The Program class is the entry point for the application.
    /// It configures and runs the web host that serves the bot application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Arguments passed from the command line.</param>
        public static void Main(string[] args)
        {
            // Creates and runs the web host
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Configures the host builder for the application.
        /// </summary>
        /// <param name="args">Arguments passed from the command line.</param>
        /// <returns>A configured IHostBuilder instance.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Specifies the startup class to be used
                    webBuilder.UseStartup<Startup>();
                });
    }
}
