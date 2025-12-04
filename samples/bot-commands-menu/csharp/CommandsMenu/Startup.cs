// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using CommandsMenu.Bots;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    // Startup class to configure services and HTTP request pipeline for the HelloWorld Bot app.
    // This class configures the bot services, localization, and routing for the app.
    // Uses SingleTenant authentication and CloudAdapter for improved authentication handling.
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // ConfigureServices method is used to add services to the DI container.
        // It configures MVC, localization, and Bot Framework services for SingleTenant setup.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix,
                opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization();

            // Add Bot Framework Authentication for SingleTenant setup
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the CloudAdapter with error handling enabled for SingleTenant authentication
            services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, CommandsMenuBot>();
        }

        // Configure method sets up the HTTP request pipeline.
        // It configures middleware for error handling, localization, routing, and static files.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<RequestLocalizationOptions> locOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();

            app.UseRequestLocalization(locOptions.Value);

            // Runs matching. An endpoint is selected and set on the HttpContext if a match is found.
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Mapping of endpoints goes here:
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
