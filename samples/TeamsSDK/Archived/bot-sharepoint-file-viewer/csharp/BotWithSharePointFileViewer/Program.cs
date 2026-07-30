// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BotWithSharePointFileViewer;
using BotWithSharePointFileViewer.Bots;
using BotWithSharePointFileViewer.Dialogs;
using BotWithSharePointFileViewer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddHttpClient();
builder.Services.AddRazorPages();

// Create the storage for User and Conversation state.
builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddSingleton<ConversationState>();

// Create a global dictionary for token information.
builder.Services.AddSingleton<ConcurrentDictionary<string, TokenState>>();

// Register bot dialog and adapter dependencies.
builder.Services.AddSingleton<MainDialog>();
builder.Services.AddSingleton<DialogManager>();
builder.Services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();
builder.Services.AddTransient<IBot, AuthBot<MainDialog>>();
builder.Services.AddSingleton<TokenExchangeHelper>();

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
app.MapRazorPages();

app.Run();