// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CallingBotSample;
using CallingBotSample.AdaptiveCards;
using CallingBotSample.Bots;
using CallingBotSample.Cache;
using CallingBotSample.Options;
using CallingBotSample.Services.BotFramework;
using CallingBotSample.Services.CognitiveServices;
using CallingBotSample.Services.MicrosoftGraph;
using CallingBotSample.Services.TeamsRecordingService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Graph.Communications.Common.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOptions();
builder.Services.AddHttpClient<ITeamsRecordingService, TeamsRecordingService>("TeamsRecordingService");

builder.Services.AddSingleton<IGraphLogger>(new GraphLogger(typeof(Program).Assembly.GetName().Name));

// Create the Bot Framework Authentication to be used with the Bot Adapter.
// Configured as single-tenant.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Bot Framework Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, MessageBot>();
builder.Services.AddTransient<CallingBot>();

builder.Services.Configure<AzureAdOptions>(builder.Configuration.GetSection("AzureAd"));
builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("Bot"));
builder.Services.Configure<CognitiveServicesOptions>(builder.Configuration.GetSection("CognitiveServices"));
builder.Services.Configure<UsersOptions>(builder.Configuration.GetSection("Users"));

builder.Services.AddSingleton<IAdaptiveCardFactory, AdaptiveCardFactory>();
builder.Services.AddMicrosoftGraphServices(options => builder.Configuration.Bind("AzureAd", options));

builder.Services.AddSingleton<IConnectorClientFactory, ConnectorClientFactory>();
builder.Services.AddScoped<ISpeechService, SpeechService>();

builder.Services.AddTransient<IBotService, BotService>();
builder.Services.AddCaches();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCookiePolicy();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
