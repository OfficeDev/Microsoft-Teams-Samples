// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.BotBuilderSamples.Bots;
using Microsoft.Extensions.Hosting;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.BotBuilderSamples
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
            // Configure HttpClient and add necessary JSON settings for the bot
            services.AddHttpClient()
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
                });

            // Register the Bot Framework Authentication service (for authentication with the Bot Framework).
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Register the Bot Adapter with error handling enabled (helps in handling errors globally).
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // The Bot needs an HttpClient to download and upload files, registered above.
            // No need to register HttpClient again, as it's already added above.

            // Register the bot implementation (TeamsFileUploadBot) as a transient service.
            services.AddTransient<IBot, TeamsFileUploadBot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles() // Enable serving static files (like images, CSS, etc.)
                .UseRouting() // Set up routing for controllers
                .UseAuthorization() // Ensure authorization is handled
                .UseEndpoints(endpoints =>
                {
                    // Map controller routes
                    endpoints.MapControllers();
                });

            // Uncomment the line below to enable HTTPS redirection in production
            // app.UseHttpsRedirection();
        }
    }
}
