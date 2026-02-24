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

// TeamsSettings from SDK handles ClientId, ClientSecret, TenantId
// ConnectionName is additional config needed for OAuth
var connectionName = builder.Configuration["Teams:ConnectionName"]
    ?? throw new InvalidOperationException("Teams:ConnectionName is not configured. Please set it in appsettings.json or environment variables.");

builder.AddTeams(App.Builder().AddOAuth(connectionName));

var app = builder.Build();
var teams = app.UseTeams();

string GetCleanMessageText(MessageActivity activity)
{
    var text = activity.Text ?? "";
    text = System.Text.RegularExpressions.Regex.Replace(text, @"<at>.*?</at>", "");
    return text.Trim().ToLowerInvariant();
}

// Welcome message
teams.OnInstall(async context =>
{
    await context.Send("Welcome to TeamsBot.");
});

// Handle sign-in completion
teams.OnSignIn(async (_, @event) =>
{
    var context = @event.Context;
    var graphClient = context.GetUserGraphClient();
    var me = await graphClient.Me.GetAsync();
    var jobTitle = me?.JobTitle ?? "Not specified";

    await context.Send($"You're signed in as {me!.DisplayName} ({me.UserPrincipalName}). Your job title is: {jobTitle}");
});

// Handle all messages
teams.OnMessage(async context =>
{
    var textLower = GetCleanMessageText(context.Activity);

    // Handle logout
    if (textLower == "logout" || textLower == "signout")
    {
        if (!context.IsSignedIn)
        {
            await context.Send("You are not signed in.");
            return;
        }
        await context.SignOut();
        await context.Send("You have been signed out.");
        return;
    }

    // Handle explicit login command
    if (textLower == "login" || textLower == "signin")
    {
        if (context.IsSignedIn)
        {
            await context.Send("You are already signed in.");
            return;
        }
        await context.SignIn();
        return;
    }

    // If not signed in, prompt to sign in
    if (!context.IsSignedIn)
    {
        await context.Send("Please type 'login' to sign in first.");
        return;
    }

    // Default response for signed-in users
    await context.Send("You are signed in. Type 'logout' to sign out.");
});

app.Run();
