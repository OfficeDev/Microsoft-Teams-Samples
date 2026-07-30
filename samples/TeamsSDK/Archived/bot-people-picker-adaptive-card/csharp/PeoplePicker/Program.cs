// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using PeoplePicker;
using PeoplePicker.Bots;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddHttpClient();

// Register Bot Framework Authentication (handles SingleTenant/MultiTenant correctly).
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Register the Bot Framework Adapter with error handling.
builder.Services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

// Register in-memory storage for User and Conversation state.
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Register ConversationState, which manages the state of conversations.
builder.Services.AddSingleton<ConversationState>();

// Register the bot as a transient service, ensuring a new instance is created per request.
builder.Services.AddTransient<IBot, ActivityBot>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
