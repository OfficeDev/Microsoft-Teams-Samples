// <copyright file="Startup.cs" company="Microsoft Corp.">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Samples.PeoplePicker
{
    using System;
    using Azure.Identity;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Graph;
    using Microsoft.Teams.Samples.PeoplePicker.Bots;
    using Microsoft.Teams.Samples.PeoplePicker.Entities;
    using Microsoft.Teams.Samples.PeoplePicker.Services;

    /// <summary>
    /// Startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="configuration">configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets IConfiguration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">IServiceCollection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHttpClient();
            services.AddHttpContextAccessor();

            // Bot dependencies.
            // using addOptions to bind configurations.
            services.AddOptions<AppSettings>().Bind(this.Configuration.GetSection("AzureAd"));
            services.AddOptions<AppSettings>().Bind(this.Configuration.GetSection("TeamsBot"));
            services.AddOptions<AppSettings>().Bind(this.Configuration.GetSection("GraphApi"));

            // Inject Graph Service Client.
            services.AddSingleton<GraphServiceClient>(this.GetAuthenticatedGraphClient());

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Creates Singleton Card Factory.
            services.AddSingleton<ICardFactory, CardFactory>();

            // Creates Singleton Task Module Response Factory.
            services.AddSingleton<ITaskModuleResponseFactory, TaskModuleResponseFactory>();

            // Creates Transient Conversation Service.
            services.AddTransient<IConversationService, ConversationService>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, TeamsBot>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">IApplicationBuilder.</param>
        /// <param name="env">IWebHostEnvironment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// Initialize new GraphServiceClient with Client Credentials.
        /// The client credentials flow requires that you request the
        /// .default scope, and preconfigure your permissions on the
        /// app registration in Azure. An administrator must grant consent
        /// to those permissions beforehand.
        /// </summary>
        /// <returns><see cref="GraphServiceClient"/>.</returns>
        public GraphServiceClient GetAuthenticatedGraphClient()
        {
            // Values from app registration
            var graphServiceClientConfig = new
            {
                TenantId = this.Configuration.GetValue<string>("AzureAd:TenantId"),
                ClientId = this.Configuration.GetValue<string>("MicrosoftAppId"),
                ClientSecret = this.Configuration.GetValue<string>("MicrosoftAppPassword"),
                Scope = this.Configuration.GetValue<string>("GraphApi:Scope"),
            };

            // using Azure.Identity;
            TokenCredentialOptions options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(graphServiceClientConfig.TenantId, graphServiceClientConfig.ClientId, graphServiceClientConfig.ClientSecret, options);
            var graphServiceClient = new GraphServiceClient(clientSecretCredential, new[] { graphServiceClientConfig.Scope });

            return graphServiceClient;
        }
    }
}
