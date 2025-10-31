// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using ChangeNotification.Bots;
using ChangeNotification.Helper;
using ChangeNotification.Model.Configuration;
using ChangeNotification.Provider;
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
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using ChangeNotification.Bots;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddTransient<GraphClient>();

builder.Services.AddTransient<SubscriptionManager>();

// Adds application configuration settings to specified IServiceCollection.
builder.Services.AddOptions<ApplicationConfiguration>()
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

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";
    if (app.Environment.IsDevelopment())
    {
        spa.UseReactDevelopmentServer(npmScript: "start");
    }
});

app.Run();