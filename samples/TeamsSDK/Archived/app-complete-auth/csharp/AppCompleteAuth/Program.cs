// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using AppCompleteAuth.Bots;
using AppCompleteAuth.Dialogs;
using AppCompleteAuth.helper;
using AppCompleteAuth.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddMvc().AddSessionStateTempDataProvider();

// Create a global hashset for Roster and Token information
builder.Services.AddSingleton<ConcurrentDictionary<string, Token>>();
builder.Services.AddSingleton<ConcurrentDictionary<string, bool>>();

builder.Services.AddHttpClient().AddControllers().AddNewtonsoftJson();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
builder.Services.AddHttpContextAccessor();

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var azureAdOptions = new AzureAdOptions();
    builder.Configuration.Bind("AzureAd", azureAdOptions);
    options.Authority = $"{azureAdOptions.Instance}{azureAdOptions.TenantId}/v2.0";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidAudiences = AuthHelper.GetValidAudiences(builder.Configuration),
        ValidIssuers = AuthHelper.GetValidIssuers(builder.Configuration),
        AudienceValidator = AuthHelper.AudienceValidator
    };
});

// Create the storage we'll be using for User and Conversation state (Memory is great for testing purposes)
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Create the Conversation state (Used by the Dialog system itself)
builder.Services.AddSingleton<ConversationState>();

// The Dialog that will be run by the bot
builder.Services.AddSingleton<MainDialog>();

// Create the Bot Framework Authentication to be used with the Bot Adapter
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Bot Framework Adapter with error handling enabled
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot
builder.Services.AddTransient<IBot, AuthBot<MainDialog>>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
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

app.Run();