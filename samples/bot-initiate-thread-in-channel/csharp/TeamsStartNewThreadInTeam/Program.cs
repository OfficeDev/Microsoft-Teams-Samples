// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// The entry point for the application that configures and runs the web host.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for the application. Initializes the web host and runs it.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        public static void Main(string[] args)
        {
            // Builds and runs the host to start the web application.
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Configures the host builder for the application.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the host builder.</param>
        /// <returns>An IHostBuilder instance configured for the application.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging(logging =>
                    {
                        // Add debug and console logging for monitoring.
                        logging.AddDebug();
                        logging.AddConsole();
                    });

                    // Use Startup class for application configuration.
                    webBuilder.UseStartup<Startup>();
                });
    }
}
