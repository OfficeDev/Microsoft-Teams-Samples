// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class Program
    {
        // The entry point of the application.
        // This method is called when the application starts.
        // It builds and runs the web host.
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Configures the web host for the application.
        // Adds logging services (Console and Debug) and sets up the Startup class.
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging((logging) =>
                    {
                        // Add debug and console logging for local development.
                        logging.AddDebug();
                        logging.AddConsole();
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
