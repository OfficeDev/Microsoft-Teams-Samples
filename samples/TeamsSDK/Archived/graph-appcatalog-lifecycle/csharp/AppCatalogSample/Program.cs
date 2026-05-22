// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AppCatalogSample.Bots;
using AppCatalogSample.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();

// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Bot Framework Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the storage we'll be using for User and Conversation state.
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Create the User state.
builder.Services.AddSingleton<UserState>();

// Create the Conversation state.
builder.Services.AddSingleton<ConversationState>();

// The Dialog that will be run by the bot.
builder.Services.AddSingleton<MainDialog>();

// Create the bot as a transient.
builder.Services.AddTransient<IBot, AppCatalogBot<MainDialog>>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles()
    .UseStaticFiles()
    .UseRouting()
    .UseAuthorization();

app.MapControllers();

app.Run();
