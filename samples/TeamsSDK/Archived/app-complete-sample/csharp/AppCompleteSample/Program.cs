// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AppCompleteSample;
using AppCompleteSample.Bots;
using AppCompleteSample.Dialogs;
using AppCompleteSample.Utility;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});

builder.Services.AddControllers();

// Create the Bot Framework Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Create the User state. (Used in this bot's Dialog implementation.)
builder.Services.AddSingleton<UserState>();

// Create the Conversation state. (Used by the Dialog system itself.)
builder.Services.AddSingleton<ConversationState>();

builder.Services.AddSingleton<PrivateConversationState>();

// The Dialog that will be run by the bot.
builder.Services.AddSingleton<RootDialog>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, DialogBot<RootDialog>>();

builder.Services.AddOptions<AzureSettings>().Configure<IConfiguration>((botOptions, configuration) =>
{
    botOptions.BotId = configuration.GetValue<string>("BotId");
    botOptions.MicrosoftAppId = configuration.GetValue<string>("MicrosoftAppId");
    botOptions.MicrosoftAppPassword = configuration.GetValue<string>("MicrosoftAppPassword");
    botOptions.MicrosoftAppType = configuration.GetValue<string>("MicrosoftAppType");
    botOptions.MicrosoftAppTenantId = configuration.GetValue<string>("MicrosoftAppTenantId");
    botOptions.BaseUri = configuration.GetValue<string>("BaseUri");
    botOptions.FBConnectionName = configuration.GetValue<string>("FBConnectionName");
    botOptions.FBProfileUrl = configuration.GetValue<string>("FBProfileUrl");
    botOptions.MaxComposeExtensionHistoryCount = configuration.GetValue<int>("MaxComposeExtensionHistoryCount");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=BotInfo}/{action=BotInfo}/{id?}");

app.Run();
