// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using MeetingNotification.Bots;
using MeetingNotification.Helper;
using MeetingNotification.Model.Configuration;
using MeetingNotification.Provider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddTransient<GraphBetaClient>();

builder.Services.AddTransient<SubscriptionHelper>();

// Adds application configuration settings to specified IServiceCollection.
builder.Services.AddOptions<BotConfiguration>()
.Configure<IConfiguration>((botOptions, configuration) =>
{
    botOptions.MicrosoftAppId = configuration.GetValue<string>("MicrosoftAppId");
    botOptions.MicrosoftAppPassword = configuration.GetValue<string>("MicrosoftAppPassword");
    botOptions.MicrosoftAppTenantId = configuration.GetValue<string>("MicrosoftAppTenantId");
    botOptions.BaseUrl = configuration.GetValue<string>("BaseUrl");
    botOptions.CertificateThumbprint = configuration.GetValue<string>("CertificateThumbprint");
    botOptions.Base64EncodedCertificate = configuration.GetValue<string>("Base64EncodedCertificate");
    botOptions.EncryptionCertificateId = configuration.GetValue<string>("EncryptionCertificateId");
});

// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Bot Framework Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Create a global hashset for our ConversationReferences
builder.Services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, MeetingNotificationBot>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles()
    .UseStaticFiles()
    .UseRouting()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

app.Run();