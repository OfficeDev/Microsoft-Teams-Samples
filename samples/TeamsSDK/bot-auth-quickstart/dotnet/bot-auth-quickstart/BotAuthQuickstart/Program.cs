// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Teams.Apps;

var builder = WebApplication.CreateBuilder(args);

// Must match the OAuth connection name configured on the Azure Bot resource.
var connectionName = builder.Configuration["OAuth:ConnectionName"] ?? "oauthbotsetting";

builder.Services.AddTeamsBotApplication(options =>
{
    options.AddOAuthFlow(connectionName, oauth =>
    {
        oauth.OAuthCardText = "Sign in to Microsoft Graph";
        oauth.SignInButtonText = "Sign in";
    });
});

var app = builder.Build();
var teams = app.UseTeamsBotApplication();
var graphAuth = teams.GetOAuthFlow(connectionName);

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("BotAuthQuickstart");

// Handle successful sign-in
graphAuth.OnSignInComplete(async (context, tokenResponse, cancellationToken) =>
{
    await context.SendAsync(
        "✅ **Successfully signed in!**\n\n" +
        "You can now use these commands:\n\n" +
        "• **profile** - View your profile\n\n" +
        "• **signout** - Sign out when done",
        cancellationToken);
});

// Handle sign-in failures
graphAuth.OnSignInFailure(async (context, failure, cancellationToken) =>
{
    logger.LogError("Sign-in failed: {Code} - {Message}", failure?.Code, failure?.Message);
    await context.SendAsync("❌ Sign-in failed. Please try again.", cancellationToken);
});

teams.OnMessage("signin", async (context, cancellationToken) =>
{
    var token = await graphAuth.SignInAsync(context, cancellationToken);

    if (token is not null)
    {
        await context.SendAsync("✅ You are already signed in!", cancellationToken);
    }
});

teams.OnMessage("signout", async (context, cancellationToken) =>
{
    await graphAuth.SignOutAsync(context, cancellationToken);
    await context.SendAsync("👋 You have been signed out successfully!", cancellationToken);
});

teams.OnMessage("profile", async (context, cancellationToken) =>
{
    var token = await graphAuth.SignInAsync(context, cancellationToken);

    // A null token means an OAuth card was sent; the flow resumes on a later turn.
    if (token is null)
    {
        return;
    }

    try
    {
        var graphClient = new GraphServiceClient(
            new BaseBearerTokenAuthenticationProvider(new StaticAccessTokenProvider(token)));

        var me = await graphClient.Me.GetAsync(cancellationToken: cancellationToken);

        if (me is null)
        {
            await context.SendAsync("❌ Could not retrieve your profile information.", cancellationToken);
            return;
        }

        await context.SendAsync(
            "👤 **Your Profile**\n\n" +
            $"**Name:** {me.DisplayName ?? "N/A"}\n\n" +
            $"**Email:** {me.UserPrincipalName ?? "N/A"}\n\n" +
            $"**Job Title:** {me.JobTitle ?? "N/A"}\n\n" +
            $"**Department:** {me.Department ?? "N/A"}\n\n" +
            $"**Office:** {me.OfficeLocation ?? "N/A"}",
            cancellationToken);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error getting profile");
        await context.SendAsync($"❌ Failed to get your profile: {ex.Message}", cancellationToken);
    }
});

teams.OnMessage(async (context, cancellationToken) =>
{
    await context.SendAsync(
        "👋 **Hello! I'm a Teams Auth Quickstart and Graph bot.**\n\n" +
        "**Available commands:**\n\n" +
        "• **signin** - Sign in to your Microsoft account\n\n" +
        "• **signout** - Sign out\n\n" +
        "• **profile** - Show your profile information\n\n",
        cancellationToken);
});

app.Run();

internal sealed class StaticAccessTokenProvider(string token) : IAccessTokenProvider
{
    public AllowedHostsValidator AllowedHostsValidator { get; } = new();

    public Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default) => Task.FromResult(token);
}
