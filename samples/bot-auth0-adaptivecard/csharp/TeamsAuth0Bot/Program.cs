// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using TeamsAuth0Bot;
using TeamsAuth0Bot.Controllers;
using TeamsAuth0Bot.Services;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Bind configuration to ConfigOptions once and register it
var configOptions = new ConfigOptions
{
    Teams = builder.Configuration.GetSection("Teams").Get<TeamsConfigOptions>() ?? new TeamsConfigOptions(),
    Auth0 = builder.Configuration.GetSection("Auth0").Get<Auth0ConfigOptions>() ?? new Auth0ConfigOptions(),
    ApplicationUrl = builder.Configuration.GetValue<string>("ApplicationUrl") ?? string.Empty
};
builder.Services.AddSingleton(configOptions);

// Services
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddSingleton<TokenStore>();
builder.Services.AddSingleton<Controller>();

// Teams SDK
builder.AddTeams();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.UseTeams();
app.Run();