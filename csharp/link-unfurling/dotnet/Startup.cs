// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The hosting environment.</param>
        public Startup(IHostingEnvironment env)
        {
            env = env ?? throw new ArgumentNullException(nameof(env));

            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables(prefix: "RLU_");

            if (env.IsDevelopment())
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
#pragma warning disable CA1506 // Composition root expected to have coupling with many components.
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
                options.AppId = this.configuration.GetValue<string>("Reddit:AppId");
                options.AppPassword = this.configuration.GetValue<string>("Reddit:AppPassword");
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
                .AddTransient<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>();

            services
                .AddHttpClient<IRedditAuthenticator, RedditAppAuthenticator>();

            services
                .AddTransient<IBot, RLUTeamsActivityHandler>();

            services
                .AddDistributedMemoryCache();

            services
                .AddHttpClient<RedditHttpClient>();

            // Add i8n
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var defaultCulture = CultureInfo.GetCultureInfo(this.configuration.GetValue<string>("I8n:DefaultCulture"));
                var supportedCultures = this.configuration.GetValue<string>("I8n:SupportedCultures").Split(',')
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
#pragma warning restore CA1506

        /// <summary>
        /// Configure the application request pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
#pragma warning disable CA1822 // This method is provided by the framework
        public void Configure(IApplicationBuilder app)
#pragma warning restore CA1822
        {
            app.UseRequestLocalization();
            app.UseMvc();
        }
    }
}
