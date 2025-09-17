// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BotWithSharePointFileViewer.Bots;
using BotWithSharePointFileViewer.Dialogs;
using System.Collections.Concurrent;
using BotWithSharePointFileViewer.Models;
using Microsoft.Bot.Builder.Dialogs;

namespace BotWithSharePointFileViewer
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
            services.AddControllers().AddNewtonsoftJson();

            services.AddHttpClient().AddControllers().AddNewtonsoftJson();
            services.AddRazorPages();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Create a global hashset for our Roster and notes information
            services.AddSingleton<ConcurrentDictionary<string, TokenState>>();

            // The Dialog that will be run by the bot.
            services.AddSingleton<MainDialog>();
            services.AddSingleton<DialogManager>();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, AuthBot<MainDialog>>();

            // Register the Token Exchange Helper, for processing TokenExchangeOperation Invoke Activities 
            services.AddSingleton<TokenExchangeHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapRazorPages();
                });
        }
    }
}
