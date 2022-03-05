// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IdentityLinkingWithSSO.Bots;
using IdentityLinkingWithSSO.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using System;
using Microsoft.AspNetCore.Http;
using IdentityLinkingWithSSO.helper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Microsoft.Bot.Connector.Authentication;
using System.Collections.Concurrent;
using IdentityLinkingWithSSO.Models;
using System.Collections.Generic;

namespace IdentityLinkingWithSSO
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
            services.AddControllers().AddNewtonsoftJson(options => {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddMemoryCache();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // Create a global hashset for our save task details
            services.AddSingleton<ConcurrentDictionary<string, List<UserMapData>>>();
            services.AddSingleton <ConcurrentDictionary<string, bool>>();
            services.AddMvc().AddSessionStateTempDataProvider();
            // Create a global hashset for our Roster and notes information
            services.AddSingleton<ConcurrentDictionary<string, Token>>();
            services.AddHttpClient().AddControllers().AddNewtonsoftJson();
            services.AddRazorPages();
            services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
            services.AddHttpContextAccessor();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    var azureAdOptions = new AzureADOptions();
                    Configuration.Bind("AzureAd", azureAdOptions);
                    options.Authority = $"{azureAdOptions.Instance}{azureAdOptions.TenantId}/v2.0";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudiences = AuthHelper.GetValidAudiences(Configuration),
                        ValidIssuers = AuthHelper.GetValidIssuers(Configuration),
                        AudienceValidator = AuthHelper.AudienceValidator
                    };
                });

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Dialog Manager handles initiating the Dialog Stack, saving state, etc.
            services.AddSingleton<DialogManager>();

            // The Dialog that will be run by the bot.
            services.AddSingleton<MainDialog>();

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

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