// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using JoinTeamByQR;
using JoinTeamByQR.Bots;
using JoinTeamByQR.Dialogs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

// Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Create the Conversation state. (Used by the Dialog system itself.)
builder.Services.AddSingleton<ConversationState>();

// Create a global dictionary for storing authentication tokens
builder.Services.AddSingleton<ConcurrentDictionary<string, string>>();

// The Dialog that will be run by the bot.
builder.Services.AddSingleton<MainDialog>();

// Register BotFrameworkAuthentication for SingleTenant
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the CloudAdapter with error handling enabled.
builder.Services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, AuthBot<MainDialog>>();

// Register the Token Exchange Helper, for processing TokenExchangeOperation Invoke Activities
builder.Services.AddSingleton<TokenExchangeHelper>();

var app = builder.Build();

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