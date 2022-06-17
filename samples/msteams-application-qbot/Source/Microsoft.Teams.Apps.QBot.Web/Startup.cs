// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.QBot.Web
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Reflection;
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
    using Microsoft.Teams.Apps.QBot.Domain;
    using Microsoft.Teams.Apps.QBot.Infrastructure.BackgroundServices;
    using Microsoft.Teams.Apps.QBot.Infrastructure.Data;
    using Microsoft.Teams.Apps.QBot.Infrastructure.QnAService;
    using Microsoft.Teams.Apps.QBot.Infrastructure.Resources;
    using Microsoft.Teams.Apps.QBot.Infrastructure.Secrets;
    using Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService;
    using Microsoft.Teams.Apps.QBot.Web.Authorization;
    using Microsoft.Teams.Apps.QBot.Web.Bot;
    using Microsoft.Teams.Apps.QBot.Web.Middlewares;
    using Microsoft.Teams.Apps.QBot.Web.SpaHost;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// The Startup class is reponsible for configuring the DI container and acts as the composition root.
    /// </summary>
    public sealed class Startup
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The environment provided configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Configure the DI container for the application.
        /// </summary>
        /// <param name="services">The stub DI container.</param>
        /// <remarks>
        /// This is the composition root of the application.
        /// For more information see: https://go.microsoft.com/fwlink/?LinkID=398940.
        /// </remarks>
        public void ConfigureServices(IServiceCollection services)
        {
            // Don't remap the claims
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // Application Insights
            services.AddApplicationInsightsTelemetry();

            // Authentication - Identity.Web, Graph SDK
            services
                .AddMicrosoftIdentityWebApiAuthentication(this.configuration)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddMicrosoftGraph(this.configuration.GetSection("GraphApiBeta"))
                .AddInMemoryTokenCaches();

            // Authorization
            services.AddCustomAuthorization(this.configuration);

            // Controllers
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                   options.SerializerSettings.Converters
                   .Add(new StringEnumConverter(new DefaultNamingStrategy(), false)));

            services.AddScoped<ErrorResponseFilterAttribute>();

            // Localization
            services.AddResources(this.configuration);

            // Register the Swagger generater.
            services.AddSwaggerGen(configuration =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.XML";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                configuration.IncludeXmlComments(xmlPath);
            });

            // Bot dependencies
            var appId = this.configuration.GetValue<string>("AzureAd:ClientId");
            var baseUrl = this.configuration.GetValue<string>("BaseUrl");
            var tenantId = this.configuration.GetValue<string>("AzureAd:TenantId");
            var botName = this.configuration.GetValue<string>("TeamsBot:AppName");
            var botAppId = this.configuration.GetValue<string>("TeamsBot:AppId");
            var manifestAppId = this.configuration.GetValue<string>("TeamsBot:ManifestAppid");
            var botCertName = this.configuration.GetValue<string>("TeamsBot:KeyVaultCertificateName");

            var appSettings = new AppSettings
            {
                BaseUrl = baseUrl,
                GraphAppId = appId,
                TenantId = tenantId,
                BotName = botName,
                BotAppId = botAppId,
                BotCertificateName = botCertName,
                ManifestAppId = manifestAppId,
            };

            var credentialProvider = new SimpleCredentialProvider()
            {
                AppId = botAppId,
            };

            services.AddSingleton<IAppSettings>(appSettings);
            services.AddSingleton<ICredentialProvider>(credentialProvider);
            services.AddTransient<BotAuthMiddleware>();
            services.AddTransient<IUrlProvider, UrlProvider>();
            services.AddTransient<IQBotTeamInfo, QBotTeamInfo>();
            services.AddTransient<IBot, BotActivityHandler>();
            services.AddTransient<BotFrameworkHttpAdapter, BotHttpAdapter>();

            // Infrastructure
            services.AddSqlServerStorage(this.configuration);
            services.AddTeamsServices();
            services.AddQnAService(this.configuration);
            services.AddBackgroundServices(this.configuration);
            services.AddSecretsProvider(this.configuration);

            // Domain
            services.AddDomainServices();

            // SPA
            services.Configure<SpaHostConfiguration>(opts =>
            {
                opts.ApplicationInsightsInstrumentationKey = this.configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
                opts.BotAppName = this.configuration.GetValue<string>("TeamsBot:AppName");
            });

            // In production, the React files will be served from this directory
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        /// <summary>
        /// Configure the application request pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Enable SwaggerUI.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRequestLocalization();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRequestContext();

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
