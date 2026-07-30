// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using ChangeNotification.Bots;
using ChangeNotification.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddDebug();
builder.Logging.AddConsole();

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
builder.Services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();
builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddSingleton<UserState>();
builder.Services.AddSingleton<ConversationState>();
builder.Services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();
builder.Services.AddSingleton<MainDialog>();
builder.Services.AddTransient<IBot, ChangeNotificationBot<MainDialog>>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
