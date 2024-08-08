// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using Botconfiguration.Bots;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(opt => { opt.ResourcesPath = "Resources"; });
            services.Configure<RequestLocalizationOptions>(Options =>
            {
                var culturSupported = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("fr-CA"),
                    new CultureInfo("hi-IN"),
                    new CultureInfo("es-MX")
                };
                Options.DefaultRequestCulture = new RequestCulture("en-US");
                Options.SupportedCultures = culturSupported;
                Options.SupportedUICultures = culturSupported;
                Options.FallBackToParentCultures = false;
            });

            services.AddControllers();
            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix,
                opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, TeamsBot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
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

            //app.UseHttpsRedirection();
        }
    }
}