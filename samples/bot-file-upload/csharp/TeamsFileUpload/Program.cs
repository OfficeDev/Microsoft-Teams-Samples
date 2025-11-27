// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using TeamsFileUpload;
using TeamsFileUpload.Controllers;
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

builder.Services.AddHttpClient();
builder.Services.AddSingleton<Controller>();
builder.AddTeams(appBuilder);

var app = builder.Build();
app.UseTeams();
app.Run();