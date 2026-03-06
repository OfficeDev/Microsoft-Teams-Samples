// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Events;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Extensions.Graph;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

var connectionName = builder.Configuration["ConnectionName"];

builder.AddTeams(App.Builder().AddOAuth(connectionName));

var app = builder.Build();
var teams = app.UseTeams();

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("BotAuthQuickstart");

string GetCleanMessageText(MessageActivity activity)
{
    var text = activity.Text ?? "";
    text = System.Text.RegularExpressions.Regex.Replace(text, @"<at>.*?</at>", "");
    return text.Trim().ToLowerInvariant();
}

// Handle successful sign-in events
teams.OnSignIn(async (_, @event) =>
{
    var context = @event.Context;
    await context.Send(
        "✅ **Successfully signed in!**\n\n" +
        "You can now use these commands:\n\n" +
        "• **profile** - View your profile\n\n" +
        "• **signout** - Sign out when done"
    );
});

// Handle all messages - route to appropriate handler based on message pattern
teams.OnMessage(async context =>
{
    // Helper function to handle authentication and create Graph client using Token pattern.
    async Task<Microsoft.Graph.GraphServiceClient?> GetAuthenticatedGraphClient()
    {
        if (!context.IsSignedIn)
        {
            await context.Send("🔐 Please sign in first to access Microsoft Graph.");
            await context.SignIn();
            return null;
        }

        try
        {
            return context.GetUserGraphClient();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create Graph client");
            await context.Send("🔐 Failed to create authenticated client. Trying to sign in again.");
            await context.SignIn();
            return null;
        }
    }

    // Handle sign-in command
    async Task HandleSignInCommand()
    {
        if (context.IsSignedIn)
        {
            await context.Send("✅ You are already signed in!");
        }
        else
        {
            await context.Send("🔐 Signing you in to access Microsoft Graph...");
            await context.SignIn();
        }
    }

    // Handle sign-out command
    async Task HandleSignOutCommand()
    {
        if (!context.IsSignedIn)
        {
            await context.Send("ℹ️ You are not currently signed in.");
        }
        else
        {
            await context.SignOut();
            await context.Send("👋 You have been signed out successfully!");
        }
    }

    // Handle profile command using Graph API with TokenProtocol pattern.
    async Task HandleProfileCommand()
    {
        try
        {
            var graphClient = await GetAuthenticatedGraphClient();
            if (graphClient == null)
            {
                return;
            }

            var me = await graphClient.Me.GetAsync();

            if (me != null)
            {
                var profileInfo =
                    "👤 **Your Profile**\n\n" +
                    $"**Name:** {me.DisplayName ?? "N/A"}\n\n" +
                    $"**Email:** {me.UserPrincipalName ?? "N/A"}\n\n" +
                    $"**Job Title:** {me.JobTitle ?? "N/A"}\n\n" +
                    $"**Department:** {me.Department ?? "N/A"}\n\n" +
                    $"**Office:** {me.OfficeLocation ?? "N/A"}";

                await context.Send(profileInfo);
            }
            else
            {
                await context.Send("❌ Could not retrieve your profile information.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting profile");
            await context.Send($"❌ Failed to get your profile: {ex.Message}");
        }
    }

    // Handle default message when no pattern matches
    async Task HandleDefaultMessage()
    {
        await context.Send(
            "👋 **Hello! I'm a Teams Auth Quickstart and Graph bot.**\n\n" +
            "**Available commands:**\n\n" +
            "• **signin** - Sign in to your Microsoft account\n\n" +
            "• **signout** - Sign out\n\n" +
            "• **profile** - Show your profile information\n\n"
        );
    }

    var textLower = GetCleanMessageText(context.Activity);

    switch (textLower)
    {
        case "signin":
            await HandleSignInCommand();
            break;
        case "signout":
            await HandleSignOutCommand();
            break;
        case "profile":
            await HandleProfileCommand();
            break;
        default:
            await HandleDefaultMessage();
            break;
    }
});

// Handle error events
teams.OnError(async (_, @event) =>
{
    logger.LogError(@event.Exception, "Error occurred");
});

app.Run();
