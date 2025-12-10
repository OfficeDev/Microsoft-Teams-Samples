// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCardActions;
using AdaptiveCardActions.Controllers;
using Azure.Core;
using Azure.Identity;
using Microsoft.Teams.Api.Auth;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Common.Http;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;

// Create web application builder and load configuration
var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.Get<ConfigOptions>();

// Factory function to create access tokens using managed identity credentials
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

// Create Teams app builder
var appBuilder = App.Builder();

// Configure authentication using user-assigned managed identity if specified
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

// Register controller and configure Teams services
builder.Services.AddSingleton<Controller>();
builder.AddTeams(appBuilder);

// Build and run the application
var app = builder.Build();
app.UseTeams();
app.Run();