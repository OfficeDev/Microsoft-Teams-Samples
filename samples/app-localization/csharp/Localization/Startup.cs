// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using Localization.Bots;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method is used to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add localization services and set the resources path.
            services.AddLocalization(options => { options.ResourcesPath = "Resources"; });

            // Configure request localization options (supported cultures, fallback logic, etc.)
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("fr-CA"),
                    new CultureInfo("hi-IN"),
                    new CultureInfo("es-MX")
                };
                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.FallBackToParentCultures = false; // Don't fallback to parent cultures
            });

            // Add MVC services for controller handling.
            services.AddControllers();

            // Configure MVC to use Razor views and add localization support.
            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization();

            // Register BotFrameworkAuthentication and CloudAdapter with error handling.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
            services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

            // Register the bot as a transient service.
            services.AddTransient<IBot, LocalizerBot>();
        }

        // This method configures the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Enable detailed error pages in development environment.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Enable HSTS for production to enhance security.
                app.UseHsts();
            }

            // Serve static files and default files (like index.html).
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Enable WebSockets (for real-time communication if needed).
            app.UseWebSockets();

            // Apply localization middleware.
            var localizationOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(localizationOptions.Value);

            // Configure routing and endpoint mapping.
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Map controller endpoints.
                endpoints.MapControllers();

                // Map default controller route.
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            // Optionally, uncomment the line below for HTTPS redirection in production.
            // app.UseHttpsRedirection();
        }
    }
}
