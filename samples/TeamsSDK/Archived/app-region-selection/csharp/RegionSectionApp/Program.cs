// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.BotBuilderSamples;
using Microsoft.BotBuilderSamples.Bots;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddNewtonsoftJson();

builder.Services.AddHttpClient();

builder.Configuration.AddEnvironmentVariables();

var appId = builder.Configuration["MicrosoftAppId"];
var appPassword = builder.Configuration["MicrosoftAppPassword"];
var appType = builder.Configuration["MicrosoftAppType"];
var appTenantId = builder.Configuration["MicrosoftAppTenantId"];

builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

builder.Services.AddSingleton<IStorage, MemoryStorage>();

builder.Services.AddSingleton<UserState>();

builder.Services.AddTransient<IBot, RegionSelectionBot>();

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