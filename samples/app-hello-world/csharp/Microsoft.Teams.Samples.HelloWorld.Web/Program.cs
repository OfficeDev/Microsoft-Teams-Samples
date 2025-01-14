// <copyright file="Program.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main method which is the entry point of the application.
        /// </summary>
        /// <param name="args">An array of command-line arguments.</param>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IWebHostBuilder"/> with pre-configured defaults.
        /// </summary>
        /// <param name="args">An array of command-line arguments.</param>
        /// <returns>An instance of <see cref="IWebHostBuilder"/>.</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
