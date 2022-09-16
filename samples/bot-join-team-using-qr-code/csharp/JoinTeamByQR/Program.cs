// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Bot.Connector.Authentication;
using JoinTeamByQR.Dialogs;
using JoinTeamByQR;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Concurrent;
using JoinTeamByQR.Bots;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);


builder.Services.AddControllers().AddNewtonsoftJson();

// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

builder.Services.AddHttpClient().AddControllers().AddNewtonsoftJson();
builder.Services.AddRazorPages();

// Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Create the Conversation state. (Used by the Dialog system itself.)
builder.Services.AddSingleton<ConversationState>();

// Create a global hashset for our Roster and notes information
builder.Services.AddSingleton<ConcurrentDictionary<string, string>>();

// Dialog Manager handles initiating the Dialog Stack, saving state, etc.
builder.Services.AddSingleton<DialogManager>();

// The Dialog that will be run by the bot.
builder.Services.AddSingleton<MainDialog>();

// Create the Bot Framework Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, AuthBot<MainDialog>>();

// Register the Token Exchange Helper, for processing TokenExchangeOperation Invoke Activities 
builder.Services.AddSingleton<TokenExchangeHelper>();

var app = builder.Build();

// Configure the HTTP request pipeline.l
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

    
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