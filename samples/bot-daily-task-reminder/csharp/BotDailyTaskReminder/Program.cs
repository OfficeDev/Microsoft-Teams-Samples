// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.14.0

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace BotDailyTaskReminder
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                // Create and run the host
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                // Log the error if the application fails to start
                Console.WriteLine($"Application failed to start: {ex.Message}");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Specify the Startup class to configure the application
                    webBuilder.UseStartup<Startup>();
                });
    }
}
