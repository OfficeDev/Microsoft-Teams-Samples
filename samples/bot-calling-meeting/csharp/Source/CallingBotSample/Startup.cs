// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CallingBotSample.AdaptiveCards;
using CallingBotSample.Bots;
using CallingBotSample.Cache;
using CallingBotSample.Options;
using CallingBotSample.Services.BotFramework;
using CallingBotSample.Services.CognitiveServices;
using CallingBotSample.Services.MicrosoftGraph;
using CallingBotSample.Services.TeamsRecordingService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph.Communications.Common.Telemetry;

namespace CallingBotSample
{

    public class Startup
    {
        private readonly GraphLogger logger;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            this.logger = new GraphLogger(typeof(Startup).Assembly.GetName().Name);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddOptions();
            services.AddHttpClient<ITeamsRecordingService, TeamsRecordingService>("TeamsRecordingService");

            services.AddSingleton<IGraphLogger>(this.logger);

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, MessageBot>();
            services.AddTransient<CallingBot>();

            services.Configure<AzureAdOptions>(Configuration.GetSection("AzureAd"));
            services.Configure<BotOptions>(Configuration.GetSection("Bot"));
            services.Configure<CognitiveServicesOptions>(Configuration.GetSection("CognitiveServices"));
            services.Configure<UsersOptions>(Configuration.GetSection("Users"));

            services.AddSingleton<IAdaptiveCardFactory, AdaptiveCardFactory>();
            services.AddMicrosoftGraphServices(options => Configuration.Bind("AzureAd", options));

            services.AddSingleton<IConnectorClientFactory, ConnectorClientFactory>();
            services.AddScoped<ISpeechService, SpeechService>();

            services.AddTransient<IBotService, BotService>();
            services.AddCaches();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseCookiePolicy();

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
