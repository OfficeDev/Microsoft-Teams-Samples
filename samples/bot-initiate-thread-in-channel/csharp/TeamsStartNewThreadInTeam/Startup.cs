// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.BotBuilderSamples.Bots;
using Microsoft.Extensions.Hosting;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Configures services and the HTTP request pipeline for the bot application.
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures services required by the application.
        /// </summary>
        /// <param name="services">The service collection used to register application services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure HTTP client, controllers, and JSON serialization settings.
            services.AddHttpClient()
                    .AddControllers()
                    .AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
                    });

            // Add Bot Framework Authentication, to authenticate requests to the bot.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Register the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Register the bot implementation to be used by the controller.
            services.AddTransient<IBot, TeamsStartNewThreadInTeam>();
        }

        /// <summary>
        /// Configures the HTTP request pipeline for the application.
        /// </summary>
        /// <param name="app">The application builder used to configure the request pipeline.</param>
        /// <param name="env">The hosting environment used to determine the current environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Enable developer exception page in development environment.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Configure the application's HTTP request pipeline.
            app.UseDefaultFiles()
               .UseStaticFiles()
               .UseRouting()
               .UseAuthorization()
               .UseEndpoints(endpoints =>
               {
                   endpoints.MapControllers();
               });

            // Uncomment the line below to enable HTTPS redirection if required.
            // app.UseHttpsRedirection();
        }
    }
}
