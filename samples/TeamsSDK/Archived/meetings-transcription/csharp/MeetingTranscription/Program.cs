// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using MeetingTranscription;
using MeetingTranscription.Bots;
using System.Collections.Concurrent;
using MeetingTranscription.Models.Configuration;
using MeetingTranscription.Services;

var builder = WebApplication.CreateBuilder(args);

// Adds application configuration settings to specified IServiceCollection.
builder.Services.AddOptions<AzureSettings>()
.Configure<IConfiguration>((botOptions, configuration) =>
{
    botOptions.MicrosoftAppId = configuration.GetValue<string>("MicrosoftAppId");
    botOptions.MicrosoftAppPassword = configuration.GetValue<string>("MicrosoftAppPassword");
    botOptions.MicrosoftAppTenantId = configuration.GetValue<string>("MicrosoftAppTenantId");
    botOptions.AppBaseUrl = configuration.GetValue<string>("AppBaseUrl");
    botOptions.GraphApiEndpoint = configuration.GetValue<string>("GraphApiEndpoint");
});

builder.Services.AddHttpClient().AddControllers().AddNewtonsoftJson();

// Creates Singleton Card Factory.
builder.Services.AddSingleton<ICardFactory, CardFactory>();

// Create a global hashset for our save task details
builder.Services.AddSingleton<ConcurrentDictionary<string, string>>();

// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Bot Adapter with error handling enabled.
builder.Services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, TranscriptionBot>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles()
    .UseStaticFiles()
    .UseWebSockets()
    .UseRouting()
    .UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();