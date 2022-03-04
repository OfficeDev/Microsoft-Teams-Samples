// <copyright file="Startup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurling
{
    using System.IdentityModel.Tokens.Jwt;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Identity.Web;
    using Microsoft.Teams.Samples.LinkUnfurling.Domain;
    using Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.ResourceServices;
    using Microsoft.Teams.Samples.LinkUnfurling.Infrastructure.TeamsServices;
    using Microsoft.Teams.Samples.LinkUnfurling.Web.Bot;
    using Microsoft.Teams.Samples.LinkUnfurling.Web.Errors;
    using Microsoft.Teams.Samples.LinkUnfurling.Web.ResponseCache;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Startup.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                   options.SerializerSettings.Converters
                   .Add(new StringEnumConverter(new DefaultNamingStrategy(), false)));

            services.AddScoped<ErrorResponseFilterAttribute>();

            // Application Insights
            services.AddApplicationInsightsTelemetry();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            // Don't remap the claims
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // Authentication - Identity.Web, Graph SDK
            services
                .AddMicrosoftIdentityWebApiAuthentication(this.configuration)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddMicrosoftGraph(this.configuration.GetSection("GraphApiBeta"))
                .AddDistributedTokenCaches();

            // Bot dependencies
            var appSettings = new AppSettings
            {
                BaseUrl = this.configuration.GetValue<string>("BaseUrl"),
                GraphAppId = this.configuration.GetValue<string>("AzureAd:ClientId"),
                TenantId = this.configuration.GetValue<string>("AzureAd:TenantId"),
                BotName = this.configuration.GetValue<string>("TeamsBot:AppName"),
                BotAppId = this.configuration.GetValue<string>("TeamsBot:AppId"),
                GraphConnectionName = this.configuration.GetValue<string>("TeamsBot:ConnectionName"),
                CatalogAppId = this.configuration.GetValue<string>("TeamsBot:CatalogAppId"),
            };

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            services.AddSingleton<IAppSettings>(appSettings);
            services.AddSingleton<IBot, BotActivityHandler>();
            services.AddSingleton<IBotFrameworkHttpAdapter, BotHttpAdapter>();
            services.AddSingleton<IMeetingResponseCache, MeetingResponseCache>();

            // Domain
            services.AddDomainServices();

            // Infrastructure.
            services.AddTeamsServices(this.configuration);
            services.AddResourceServices(this.configuration);
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
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
