using JoinTeamByQR;
using JoinTeamByQR.Controllers;
using Azure.Core;
using Azure.Identity;
using Microsoft.Teams.Api.Auth;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Events;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Common.Http;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Get configuration from appsettings.json / appsettings.Development.json
var config = builder.Configuration.Get<ConfigOptions>() ?? new ConfigOptions();

// Create a global dictionary for token cache
builder.Services.AddSingleton<ConcurrentDictionary<string, string>>();

// Register configuration as singleton
builder.Services.AddSingleton(config);

Func<string[], string, Task<ITokenResponse>> createTokenFactory = async (string[] scopes, string tenantId) =>
{
    var clientId = config.Teams.ClientId;

    if (config.Teams.BotType == "UserAssignedMsi")
    {
        var managedIdentityCredential = new ManagedIdentityCredential(clientId);
        var tokenRequestContext = new TokenRequestContext(scopes, tenantId: tenantId);
        var accessToken = await managedIdentityCredential.GetTokenAsync(tokenRequestContext);

        return new TokenResponse
        {
            TokenType = "Bearer",
            AccessToken = accessToken.Token,
        };
    }
    else
    {
        // For ClientSecret authentication
        var credential = new ClientSecretCredential(
            config.Teams.TenantId,
            config.Teams.ClientId,
            config.Teams.ClientSecret
        );
        
        var tokenRequestContext = new TokenRequestContext(scopes, tenantId: tenantId);
        var accessToken = await credential.GetTokenAsync(tokenRequestContext);

        return new TokenResponse
        {
            TokenType = "Bearer",
            AccessToken = accessToken.Token,
        };
    }
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
else if (config.Teams.BotType == "ClientSecret")
{
    appBuilder.AddCredentials(new TokenCredentials(
        config.Teams.ClientId ?? string.Empty,
        async (tenantId, scopes) =>
        {
            return await createTokenFactory(scopes, tenantId);
        }
    ));
}

// Add OAuth for user authentication
appBuilder.AddOAuth(config.Teams.ConnectionName);

// Register the controller for TaskFetch/TaskSubmit handling
builder.Services.AddSingleton<Controller>();
builder.AddTeams(appBuilder);

var app = builder.Build();

// Enable serving static files
app.UseDefaultFiles();
app.UseStaticFiles();

// Enable routing
app.UseRouting();

var teams = app.UseTeams();

// Get token cache from DI
var tokenCache = app.Services.GetRequiredService<ConcurrentDictionary<string, string>>();

// Handle sign-in message command
teams.OnMessage("/signin", async context =>
{
    if (context.IsSignedIn)
    {
        await context.Send("You are already signed in!");
        return;
    }
    
    await context.SignIn();
});

// Handle sign-out message command
teams.OnMessage("/signout", async context =>
{
    if (!context.IsSignedIn)
    {
        await context.Send("You are not signed in!");
        return;
    }

    // Clear token from cache
    var userId = context.Activity.From.Id;
    tokenCache.TryRemove($"Token_{userId}", out _);
    tokenCache.TryRemove("Token", out _);
    
    await context.SignOut();
    await context.Send("You have been signed out!");
});

// Handle 'generate' message command
teams.OnMessage("generate", async context =>
{
    if (!context.IsSignedIn)
    {
        await context.Send("Please sign in first by typing '/signin'");
        return;
    }

    // Create Adaptive Card as raw JSON object for maximum compatibility
    var cardContent = new Dictionary<string, object>
    {
        { "type", "AdaptiveCard" },
        { "$schema", "http://adaptivecards.io/schemas/adaptive-card.json" },
        { "version", "1.4" },
        { "body", new List<object>
            {
                new Dictionary<string, object>
                {
                    { "type", "TextBlock" },
                    { "text", "QR Code Generator" },
                    { "size", "Large" },
                    { "weight", "Bolder" },
                    { "wrap", true }
                },
                new Dictionary<string, object>
                {
                    { "type", "TextBlock" },
                    { "text", "Generate QR codes for teams. Other users can scan the QR code to join the team." },
                    { "size", "Medium" },
                    { "wrap", true },
                    { "spacing", "Small" }
                },
                new Dictionary<string, object>
                {
                    { "type", "TextBlock" },
                    { "text", "Tap the button below to open the QR generator." },
                    { "size", "Small" },
                    { "wrap", true },
                    { "spacing", "Medium" },
                    { "isSubtle", true }
                }
            }
        },
        { "actions", new List<object>
            {
                new Dictionary<string, object>
                {
                    { "type", "Action.Submit" },
                    { "title", "Open QR Generator" },
                    { "data", new Dictionary<string, object>
                        {
                            { "msteams", new Dictionary<string, object> { { "type", "task/fetch" } } },
                            { "opendialogtype", "qr_generator" }
                        }
                    }
                }
            }
        }
    };

    // Create attachment with raw card content
    var attachment = new Microsoft.Teams.Api.Attachment
    {
        ContentType = Microsoft.Teams.Api.ContentType.AdaptiveCard,
        Content = cardContent
    };

    // Create message activity with attachment
    var activity = new Microsoft.Teams.Api.Activities.MessageActivity
    {
        Attachments = new List<Microsoft.Teams.Api.Attachment> { attachment }
    };

    await context.Send(activity);
});

// Handle any other message - show available commands
teams.OnMessage(async context =>
{
    var userCommand = context.Activity.Text?.ToLower().Trim();

    // Skip if it's a command handled elsewhere
    if (userCommand == "/signin" || userCommand == "/signout" || userCommand == "generate")
    {
        return;
    }

    await context.Send("Available commands:\n- '/signin' - Sign in to your account\n- '/signout' - Sign out\n- 'generate' - Open QR code generator dialog");
});

// Handle successful sign-in event
teams.OnSignIn(async (_, teamsEvent) =>
{
    var context = teamsEvent.Context;
    var token = teamsEvent.Token.Token;
    var userId = context.Activity.From.Id;
    
    // Store token in cache with user ID
    tokenCache.AddOrUpdate($"Token_{userId}", token, (key, oldValue) => token);
    
    // Also store a general token (for backward compatibility with API controllers)
    tokenCache.AddOrUpdate("Token", token, (key, oldValue) => token);
    
    await context.Send("Signed in successfully! You can now type 'generate' to create QR codes for teams. Type '/signout' to sign out.");
});

// Handle members added event
teams.OnMembersAdded(async context =>
{
    var welcomeText = "Hello and welcome! With this sample your bot can generate QR code for the selected team and the user will be able to join the team by scanning QR code. Type '/signin' to authenticate, then type 'generate' to begin.";
    
    foreach (var member in context.Activity.MembersAdded)
    {
        if (member.Id != context.Activity.Recipient.Id)
        {
            await context.Send(welcomeText);
        }
    }
});

// Map controllers for API endpoints
app.MapControllers();

app.Run();