// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio v4.14.0

using PeoplePicker.Bots;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace PeoplePicker
{
    /// <summary>
    /// Configures services for dependency injection and the HTTP request pipeline.
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configuration for the application.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Adds services to the container.
        /// </summary>
        /// <param name="services">The collection of services to configure.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add necessary services for controllers, including JSON serialization.
            services.AddControllers().AddNewtonsoftJson();
            services.AddHttpClient().AddControllers().AddNewtonsoftJson();
            services.AddRazorPages();

            // Register the Bot Framework Adapter with error handling.
            services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

            // Register in-memory storage for User and Conversation state.
            services.AddSingleton<IStorage, MemoryStorage>();

            // Register ConversationState, which manages the state of conversations.
            services.AddSingleton<ConversationState>();

            // Register the bot as a transient service, ensuring a new instance is created per request.
            services.AddTransient<IBot, ActivityBot>();
        }

        /// <summary>
        /// Configures the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder to configure.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Enable detailed error pages in the development environment.
                app.UseDeveloperExceptionPage();
            }

            // Configure the app to serve static files and handle routing.
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Enable WebSocket support and configure routing and authorization.
            app.UseWebSockets()
               .UseRouting()
               .UseAuthorization()
               .UseEndpoints(endpoints =>
               {
                   // Map controllers to handle requests.
                   endpoints.MapControllers();
               });
        }
    }
}
