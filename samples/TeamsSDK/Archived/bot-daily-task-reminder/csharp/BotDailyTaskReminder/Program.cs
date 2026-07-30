// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BotDailyTaskReminder;
using BotDailyTaskReminder.Bots;
using BotDailyTaskReminder.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddRazorPages();

builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
builder.Services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();
builder.Services.AddSingleton<ConcurrentDictionary<string, List<SaveTaskDetail>>>();
builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddSingleton<ConversationState>();
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
app.MapRazorPages();

app.Run();
