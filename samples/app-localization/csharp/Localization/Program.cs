// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Build and run the web host.
            CreateWebHostBuilder(args).Build().Run();
        }

        // Creates and configures the web host for the application.
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>(); // Uses the Startup class to configure services and the request pipeline.
    }
}
