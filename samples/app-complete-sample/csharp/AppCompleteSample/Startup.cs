// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AppCompleteSample.Bots;
using System;
using AppCompleteSample.Utility;
using AppCompleteSample.Dialogs;

namespace AppCompleteSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromMinutes(60);//You can set Time   
            });
            services.AddControllers();
            services.AddMvc();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            services.AddSingleton<PrivateConversationState>();

            // The Dialog that will be run by the bot.
            services.AddSingleton<RootDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogBot<RootDialog>>();

            services.AddOptions<AzureSettings>().Configure<IConfiguration>((botOptions, configuration) =>
            {
                botOptions.BotId = configuration.GetValue<string>("BotId");
                botOptions.MicrosoftAppId = configuration.GetValue<string>("MicrosoftAppId");
                botOptions.MicrosoftAppPassword = configuration.GetValue<string>("MicrosoftAppPassword");
                botOptions.BaseUri = configuration.GetValue<string>("BaseUri");
                botOptions.FBConnectionName = configuration.GetValue<string>("FBConnectionName");
                botOptions.FBProfileUrl = configuration.GetValue<string>("FBProfileUrl");
                botOptions.MaxComposeExtensionHistoryCount = configuration.GetValue<int>("MaxComposeExtensionHistoryCount");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=BotInfo}/{action=BotInfo}/{id?}");
                });
        }

    }
}