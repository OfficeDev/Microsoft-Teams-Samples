// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    /// <summary>
    /// Configures services and middleware for the application.
    /// This class is used by the runtime to set up the application's request pipeline.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Configuration for setting up the application environment.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration for the application.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures services for the application.
        /// </summary>
        /// <param name="services">A collection of services to be added to the container.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add controllers and MVC services to the container
            services.AddControllers();  // Adds controllers to the service collection
            services.AddMvc(); // Adds MVC services for handling views and routes

            // Create the Bot Framework Authentication to be used with the Bot Adapter
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Register Bot Framework HTTP Adapter with custom error handler
            services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

            // Register the bot (MessageExtension) as a transient service
            services.AddTransient<IBot, MessageExtension>();
        }

        /// <summary>
        /// Configures the HTTP request pipeline for the application.
        /// </summary>
        /// <param name="app">The application's builder for configuring the middleware pipeline.</param>
        /// <param name="env">The environment information for the application.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Enable developer exception page if the environment is development
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Enable HTTPS and HSTS for production environments
                app.UseHsts(); // HTTP Strict Transport Security for enhanced security
                app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
            }

            // Configure static files, default files, and WebSocket support
            app.UseDefaultFiles(); // Serves default files (e.g., index.html)
            app.UseStaticFiles();  // Serves static files (e.g., images, CSS, JS)
            app.UseWebSockets();  // Enable WebSocket support

            // Configure routing for the application
            app.UseRouting();  // Enables routing capabilities

            // Configure endpoint mapping
            app.UseEndpoints(endpoints =>
            {
                // Map controller endpoints
                endpoints.MapControllers();

                // Map default route for controller actions
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");  // Default route for Home controller
            });
        }
    }
}
