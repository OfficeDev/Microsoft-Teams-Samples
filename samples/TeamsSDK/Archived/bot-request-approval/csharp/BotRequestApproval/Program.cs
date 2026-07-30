// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BotRequestApproval;
using BotRequestApproval.Bots;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddHttpClient();

// Create the CloudAdapter with error handling enabled.
builder.Services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

// Create the storage we'll be using for User and Conversation state (Memory is great for testing purposes).
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Create the Conversation state (used by the Dialog system itself).
builder.Services.AddSingleton<ConversationState>();

// Create the bot as a transient.
builder.Services.AddTransient<IBot, ActivityBot>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
