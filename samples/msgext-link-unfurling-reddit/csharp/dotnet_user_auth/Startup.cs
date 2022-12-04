// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.LinkUnfurlerForReddit
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.ApplicationInsights.AspNetCore.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The Startup class is reponsible for configuring the DI container and acts as the composition root.
    /// </summary>
    public sealed class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IHostingEnvironment env;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The hosting environment.</param>
        public Startup(IHostingEnvironment env)
        {
            this.env = env ?? throw new ArgumentNullException(nameof(env));

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables(prefix: "RLU_");

            if (this.env.IsDevelopment())
            {
                // Using dotnet secrets to store the settings during development
                // https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.0&tabs=windows
                configBuilder.AddUserSecrets<Startup>();
            }

            this.configuration = configBuilder.Build();
        }

        /// <summary>
        /// Configure the composition root for the application.
        /// </summary>
        /// <param name="services">The stub composition root.</param>
        /// <remarks>
        /// For more information see: https://go.microsoft.com/fwlink/?LinkID=398940.
        /// </remarks>
        public void ConfigureServices(IServiceCollection services)
        {
            string appId = this.configuration.GetValue<string>("App:Id");
            string appPassword = this.configuration.GetValue<string>("App:Password");

            ICredentialProvider credentialProvider = new SimpleCredentialProvider(
                appId: appId,
                password: appPassword);

            services.Configure<RedditOptions>(options =>
            {
                options.BotFrameworkConnectionName = this.configuration.GetValue<string>("BotFramework:ConnectionName");
                options.ClientUserAgent = this.configuration.GetValue<string>("UserAgent");
                options.SettingsPageUrl = this.configuration.GetValue<string>("SettingsPageUrl");
            });

            services
                .AddApplicationInsightsTelemetry();

            services.Configure<ApplicationInsightsServiceOptions>(options =>
            {
                options.InstrumentationKey = this.configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
            });

            services
                .AddApplicationInsightsTelemetry();

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services
                .AddSingleton(credentialProvider);

            services
                .AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>();

            services
                .AddTransient<IBot, RLUTeamsActivityHandler>();

            services
                .AddHttpClient<RedditHttpClient>();

            // Add internationalization suport
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var defaultCulture = CultureInfo.GetCultureInfo(this.configuration.GetValue<string>("Internationalization:DefaultCulture"));
                var supportedCultures = this.configuration.GetValue<string>("Internationalization:SupportedCultures").Split(',')
                    .Select(culture => CultureInfo.GetCultureInfo(culture))
                    .ToList();

                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new BotLocalizationCultureProvider(),
                };
            });
        }

        /// <summary>
        /// Configure the application request pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
#pragma warning disable CA1822 // This method is provided by the framework
        public void Configure(IApplicationBuilder app)
#pragma warning restore CA1822
        {
            app.UseMvc();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (this.env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
