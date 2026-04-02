// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Teams.Samples.LinkUnfurlerForReddit;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(prefix: "RLU_");

builder.Services.AddControllers();
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.Configure<RedditOptions>(options =>
{
    options.BotFrameworkConnectionName = builder.Configuration["BotFramework:ConnectionName"];
    options.ClientUserAgent = builder.Configuration["UserAgent"];
    options.AppId = builder.Configuration["Reddit:AppId"];
    options.AppPassword = builder.Configuration["Reddit:AppPassword"];
});

// Bot Framework authentication and adapter (CloudAdapter).
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, CloudAdapter>();

builder.Services.AddHttpClient<IRedditAuthenticator, RedditAppAuthenticator>();
builder.Services.AddTransient<IBot, RLUTeamsActivityHandler>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpClient<RedditHttpClient>();

// Localization.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var defaultCulture = CultureInfo.GetCultureInfo(builder.Configuration["I8n:DefaultCulture"]!);
    var supportedCultures = builder.Configuration["I8n:SupportedCultures"]!
        .Split(',')
        .Select(culture => CultureInfo.GetCultureInfo(culture))
        .ToList();

    options.DefaultRequestCulture = new RequestCulture(defaultCulture);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = [new BotLocalizationCultureProvider()];
});

var app = builder.Build();

app.UseRequestLocalization();
app.MapControllers();

app.Run();
