// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Bot.Solutions.Skills.Dialogs;
using Microsoft.Bot.Solutions.Skills.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Teams.Apps.VirtualAssistant.Adapters;
using Microsoft.Teams.Apps.VirtualAssistant.Authentication;
using Microsoft.Teams.Apps.VirtualAssistant.Bots;
using Microsoft.Teams.Apps.VirtualAssistant.Dialogs;
using Microsoft.Teams.Apps.VirtualAssistant.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("cognitivemodels.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"cognitivemodels.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Configure MVC
builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Load settings
var settings = new BotSettings();
builder.Configuration.Bind(settings);
builder.Services.AddSingleton(settings);

// Configure channel provider
builder.Services.AddSingleton<IChannelProvider, ConfigurationChannelProvider>();

// Configure configuration provider
builder.Services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

// Configure telemetry
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();
builder.Services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();
builder.Services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();
builder.Services.AddSingleton<TelemetryInitializerMiddleware>();
builder.Services.AddSingleton<TelemetryLoggerMiddleware>();

// Configure bot services
builder.Services.AddSingleton<BotServices>();

// Configure storage
// Uncomment the following line for local development without Cosmos Db
// builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddSingleton<IStorage>(_ =>
{
    return new CosmosDbPartitionedStorage(settings.CosmosDb);
});
builder.Services.AddSingleton<ConversationState>();

// Configure localized responses
var localizedTemplates = new Dictionary<string, List<string>>();
var templateFiles = new List<string> { "MainResponses" };
var supportedLocales = new List<string> { "en-us", "de-de", "es-es", "fr-fr", "it-it", "zh-cn" };

foreach (var locale in supportedLocales)
{
    var localeTemplateFiles = new List<string>();
    foreach (var template in templateFiles)
    {
        // LG template for en-us does not include locale in file extension.
        if (locale.Equals("en-us"))
        {
            localeTemplateFiles.Add(Path.Combine(".", "Responses", $"{template}.lg"));
        }
        else
        {
            localeTemplateFiles.Add(Path.Combine(".", "Responses", $"{template}.{locale}.lg"));
        }
    }

    localizedTemplates.Add(locale, localeTemplateFiles);
}

builder.Services.AddSingleton(new LocaleTemplateEngineManager(localizedTemplates, settings.DefaultLocale ?? "en-us"));

// Register the skills configuration class
builder.Services.AddSingleton<SkillsConfiguration>();

// Register AuthConfiguration to enable custom claim validation.
builder.Services.AddSingleton(sp => new AuthenticationConfiguration { ClaimsValidator = new Microsoft.Teams.Apps.VirtualAssistant.Authentication.AllowedCallersClaimsValidator(sp.GetRequiredService<SkillsConfiguration>()) });

// Register dialogs
builder.Services.AddTransient<MainDialog>();
builder.Services.AddTransient<TeamsSwitchSkillDialog>();

// Register the Bot Framework Adapter with error handling enabled.
// Note: some classes use the base BotAdapter so we add an extra registration that pulls the same instance.
builder.Services.AddSingleton<BotFrameworkHttpAdapter, DefaultAdapter>();
builder.Services.AddSingleton<BotAdapter>(sp => sp.GetRequiredService<BotFrameworkHttpAdapter>());

// Configure bot
builder.Services.AddTransient<IBot>(sp => new DefaultActivityHandler<MainDialog>(sp, sp.GetRequiredService<MainDialog>(), builder.Configuration["microsoftAppId"]));

// Register the skills conversation ID factory, the client and the request handler.
builder.Services.AddSingleton<SkillConversationIdFactoryBase, SkillConversationIdFactory>();
builder.Services.AddHttpClient<SkillHttpClient>();
builder.Services.AddSingleton<ChannelServiceHandler, SkillHandler>();

// Register the SkillDialogs (remote skills).
var section = builder.Configuration.GetSection("BotFrameworkSkills");
var skills = section.Get<EnhancedBotFrameworkSkill[]>();
if (skills != null)
{
    var hostEndpointSection = builder.Configuration.GetSection("SkillHostEndpoint");
    if (hostEndpointSection == null)
    {
        throw new ArgumentException($"{hostEndpointSection} is not in the configuration");
    }

    var hostEndpoint = new Uri(hostEndpointSection.Value!);

    foreach (var skill in skills)
    {
        builder.Services.AddSingleton(sp => new TeamsSkillDialog(sp.GetRequiredService<ConversationState>(), sp.GetRequiredService<SkillHttpClient>(), skill, builder.Configuration, hostEndpoint));
    }
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();
app.UseRouting();
app.MapControllers();

app.Run();
