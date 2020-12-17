// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Teams.Samples.LinkUnfurlerForReddit
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// The Program class is responsible for holding the entrypoint of the program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The entrypoint for the program.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Build the webhost for servicing HTTP requests.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns> The WebHostBuilder configured from the arguments with the composition root defined in <see cref="Startup" />.</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost
                .CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
