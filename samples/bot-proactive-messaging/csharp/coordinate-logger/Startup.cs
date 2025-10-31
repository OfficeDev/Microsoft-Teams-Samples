
namespace msteams_app_coordinatelogger
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Teams.CoordinateLogger.Bot;
    using Microsoft.Teams.CoordinateLogger.Services;

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
        public Startup(IWebHostEnvironment env)
        {
            var cfgBuilder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.local.json")
                .AddEnvironmentVariables();
            
            this.configuration = cfgBuilder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string appId = this.configuration.GetValue<string>("App:Id");
            string appPassword = this.configuration.GetValue<string>("App:Password");

            services
                .AddSingleton(new MicrosoftAppCredentials(appId, appPassword));
            
            services
                .AddSingleton<IConnectorClientFactory, ConnectorClientFactory>();

            services
                .AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            services
                .AddTransient<CloudAdapter>();

            services
                .AddTransient<IBot, CoordinateLoggerActivityHandler>();

            services
                .AddMvc(options => options.EnableEndpointRouting = false);

        }

        /// <summary>
        /// Configure the application request pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
        public void Configure(IApplicationBuilder app)
        {
            app.UseRequestLocalization();
            app.UseMvc();
        }
    }
}
