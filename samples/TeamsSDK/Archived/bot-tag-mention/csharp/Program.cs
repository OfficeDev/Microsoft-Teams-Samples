// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TagMentionBot;
using TagMentionBot.Bots;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddDebug().AddConsole();

builder.Services.AddHttpClient()
    .AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
    });

// Create the Bot Adapter with error handling enabled.
builder.Services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

// Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Create the User state. (Used in this bot's Dialog implementation.)
builder.Services.AddSingleton<UserState>();

// Create the Conversation state. (Used by the Dialog system itself.)
builder.Services.AddSingleton<ConversationState>();

// The Dialog that will be run by the bot.
builder.Services.AddSingleton<MainDialog>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, TeamsConversationBot<MainDialog>>();

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
