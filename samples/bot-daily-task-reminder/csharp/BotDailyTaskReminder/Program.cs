using System.Collections.Concurrent;
using BotDailyTaskReminder;
using BotDailyTaskReminder.Controllers;
using BotDailyTaskReminder.Models;
using BotDailyTaskReminder.Services;
using Azure.Core;
using Azure.Identity;
using Microsoft.Teams.Api.Auth;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Common.Http;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.Get<ConfigOptions>();

Func<string[], string?, Task<ITokenResponse>> createTokenFactory = async (string[] scopes, string? tenantId) =>
{
    var clientId = config.Teams.ClientId;

    var managedIdentityCredential = new ManagedIdentityCredential(clientId);
    var tokenRequestContext = new TokenRequestContext(scopes, tenantId: tenantId);
    var accessToken = await managedIdentityCredential.GetTokenAsync(tokenRequestContext);

    return new TokenResponse
    {
        TokenType = "Bearer",
        AccessToken = accessToken.Token,
    };
};

var appBuilder = App.Builder();

if (config.Teams.BotType == "UserAssignedMsi")
{
    appBuilder.AddCredentials(new TokenCredentials(
        config.Teams.ClientId ?? string.Empty,
        async (tenantId, scopes) =>
        {
            return await createTokenFactory(scopes, tenantId);
        }
    ));
}

// Register singleton services for state management
builder.Services.AddSingleton(new ConcurrentDictionary<string, Microsoft.Teams.Api.ConversationReference>());
builder.Services.AddSingleton(new ConcurrentDictionary<string, List<SaveTaskDetail>>());

// Add controllers for API endpoints
builder.Services.AddControllers();

// Register the Teams controller with explicit dependencies
builder.Services.AddSingleton<Controller>(sp => new Controller(
    sp.GetRequiredService<ConcurrentDictionary<string, Microsoft.Teams.Api.ConversationReference>>(),
    sp.GetRequiredService<ConcurrentDictionary<string, List<SaveTaskDetail>>>(),
    builder.Configuration
));

// Register the background service for sending reminders
builder.Services.AddHostedService<ReminderBackgroundService>();

builder.AddTeams(appBuilder);

var app = builder.Build();

// Enable static files from wwwroot
app.UseStaticFiles();

// Map controllers for API endpoints
app.MapControllers();

app.UseStaticFiles();

app.UseTeams();
app.AddTab("ScheduleTask", "wwwroot");
app.Run();
