// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    // The entry point for the HelloWorld Bot application.
    // It configures and runs the web host using the Startup class.
    public class Program
    {
        public static void Main(string[] args)
        {
            // Calls the CreateWebHostBuilder method to set up and run the application.
            CreateWebHostBuilder(args).Build().Run();
        }

        // Creates and configures the web host with the Startup class for setting up services and middleware.
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args) // Sets up default configuration, logging, etc.
                .UseStartup<Startup>(); // Uses the Startup class to configure services and the request pipeline
    }
}
