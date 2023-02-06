// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Web;

using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Teams.Samples.ConversationalTabs.Domain;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Settings;
using Microsoft.Teams.Samples.ConversationalTabs.Infrastructure;
using Microsoft.Teams.Samples.ConversationalTabs.Web.Authorization;
using Microsoft.Teams.Samples.ConversationalTabs.Web.Bot;
using Microsoft.Teams.Samples.ConversationalTabs.Web.Middleware;

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
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddMicrosoftIdentityWebApiAuthentication(Configuration, "AzureAd")
            .EnableTokenAcquisitionToCallDownstreamApi()
            // If you do not need Graph calls, remove the following.
            .AddMicrosoftGraph(Configuration.GetSection("Graph"))
            .AddSessionTokenCaches();

        services.AddHttpContextAccessor();
        services.AddCustomAuthorization();
        services.AddControllersWithViews();

        // Create the Bot Framework Authentication to be used with the Bot Adapter.
        services.AddSingleton<BotFrameworkAuthentication>(sp =>
        {
            return new ConfigurationBotFrameworkAuthentication(Configuration.GetSection("Bot"));
        });
        services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
        services.AddSingleton<IBot, BotActivityHandler>();

        services.AddDomainServices();
        services.AddInfrastructureServices();

        // In production, the React files will be served from this directory
        services.AddSpaStaticFiles(configuration =>
        {
            configuration.RootPath = "ClientApp/build";
        });

        services.Configure<BotSettings>(Configuration.GetSection("Bot"));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseSession();

        app.UseMiddleware<JsonExceptionHandler>();

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
                spa.Options.StartupTimeout = TimeSpan.FromSeconds(120);
                spa.UseReactDevelopmentServer(npmScript: "start");
            }
        });
    }
}
