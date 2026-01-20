using bot_conversation;
using bot_conversation.Controllers;
using Azure.Core;
using Azure.Identity;
using Microsoft.Teams.Api.Auth;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Common.Http;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.Get<ConfigOptions>() ?? new ConfigOptions();

#nullable enable
Func<string[], string?, Task<ITokenResponse>> createTokenFactory = async (string[] scopes, string? tenantId) =>
#nullable restore
{
    var clientId = config.Teams.ClientId;
    var clientSecret = config.Teams.ClientSecret;
    var tenantIdFromConfig = config.Teams.TenantId ?? tenantId;

    // For local development, use ClientSecretCredential
    // For Azure deployment, use ManagedIdentityCredential
    TokenCredential credential;
    
    if (!string.IsNullOrEmpty(clientSecret))
    {
        // Local development: Use Client Secret
        credential = new ClientSecretCredential(tenantIdFromConfig, clientId, clientSecret);
    }
    else
    {
        // Azure deployment: Use Managed Identity
        credential = new ManagedIdentityCredential(clientId);
    }
    
    var tokenRequestContext = new TokenRequestContext(scopes, tenantId: tenantId);
    var accessToken = await credential.GetTokenAsync(tokenRequestContext, CancellationToken.None);

    return new TokenResponse
    {
        TokenType = "Bearer",
        AccessToken = accessToken.Token,
    };
};
var appBuilder = App.Builder();

Microsoft.Teams.Common.Http.IHttpCredentials? httpCredentials = null;

if (config.Teams.BotType == "UserAssignedMsi")
{
    httpCredentials = new TokenCredentials(
        config.Teams.ClientId ?? string.Empty,
        async (tenantId, scopes) =>
        {
            return await createTokenFactory(scopes, tenantId);
        }
    );
    
    appBuilder.AddCredentials(httpCredentials);
}

// Build the Teams app
var teamsApp = appBuilder.Build();

// Register services BEFORE calling AddTeams
builder.Services.AddSingleton(teamsApp);

if (httpCredentials != null)
{
    builder.Services.AddSingleton(httpCredentials);
}

builder.Services.AddSingleton<Controller>();

// AddTeams must be called after registering custom services
builder.AddTeams(appBuilder);

var app = builder.Build();
app.UseTeams();
app.Run();