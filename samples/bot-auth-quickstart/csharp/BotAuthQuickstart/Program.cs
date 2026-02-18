// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using BotAuthQuickstart;
using Microsoft.Teams.Api.Activities;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Events;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Extensions.Graph;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.Get<ConfigOptions>();

builder.AddTeams(App.Builder().AddOAuth(config?.Teams?.ConnectionName ?? ""));

var app = builder.Build();
var teams = app.UseTeams();

var userStates = new ConcurrentDictionary<string, UserState>();
var conversationReferences = new ConcurrentDictionary<string, ConversationReferenceData>();

UserState GetUserState(string odUserId) => userStates.GetOrAdd(odUserId, _ => new UserState());
void SetUserState(string odUserId, UserState state) => userStates[odUserId] = state;

void StoreConversationReference(IActivity activity)
{
    var odUserId = activity.From?.AadObjectId ?? activity.From?.Id;
    if (string.IsNullOrEmpty(odUserId)) return;
    conversationReferences[odUserId] = new ConversationReferenceData
    {
        UserId = odUserId,
        ConversationId = activity.Conversation?.Id,
        ServiceUrl = activity.ServiceUrl,
        ChannelId = activity.ChannelId,
        TenantId = activity.Conversation?.TenantId
    };
}

string GetCleanMessageText(MessageActivity activity)
{
    var text = activity.Text ?? "";
    text = System.Text.RegularExpressions.Regex.Replace(text, @"<at>.*?</at>", "");
    return text.Trim().ToLowerInvariant();
}

// Middleware to store conversation references
teams.Use(async context =>
{
    StoreConversationReference(context.Activity);
    await context.Next();
});

// Welcome message
teams.OnMembersAdded(async context =>
{
    await context.Send(
        "Welcome to TeamsBot.");
});

// Handle sign-in completion - capture the token
teams.OnSignIn(async (_, @event) =>
{
    var context = @event.Context;
    var token = @event.Token.Token;
    var userId = context.Activity.From.Id;

    // Store the token in user state
    var userState = GetUserState(userId);
    userState.Token = token;
    SetUserState(userId, userState);

    var graphClient = context.GetUserGraphClient();
    var me = await graphClient.Me.GetAsync();
    var jobTitle = me?.JobTitle ?? "null";

    await context.Send($"You're logged in as {me!.DisplayName} ({me.UserPrincipalName}); your job title is: {jobTitle}; your photo is:");

    try
    {
        var photoStream = await graphClient.Me.Photo.Content.GetAsync();
        if (photoStream != null)
        {
            using var memoryStream = new MemoryStream();
            await photoStream.CopyToAsync(memoryStream);
            var base64Photo = Convert.ToBase64String(memoryStream.ToArray());
            await context.Send($"<img src=\"data:image/jpeg;base64,{base64Photo}\" alt=\"Profile Photo\" />");
        }
    }
    catch { /* Profile photo not available */ }

    await context.Send("Would you like to view your token?\n\n**Yes or No**");
    userState.WaitingForTokenConfirmation = true;
    SetUserState(userId, userState);
});

// Handle all messages
teams.OnMessage(async context =>
{
    var userId = context.Activity.From.Id;
    var userState = GetUserState(userId);
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

    // Handle token confirmation response
    if (userState.WaitingForTokenConfirmation && context.IsSignedIn)
    {
        if (textLower == "yes")
        {
            if (!string.IsNullOrEmpty(userState.Token))
            {
                await context.Send($"Here is your token: {userState.Token}");
            }
            else
            {
                await context.Send("Token was not captured. This can happen if the sign-in completed silently.");
            }
        }
        else
        {
            await context.Send("Thank you.");
        }
        userState.WaitingForTokenConfirmation = false;
        userState.Token = null;
        SetUserState(userId, userState);
        return;
    }

    // Handle install command - install app for team members
    if (textLower == "install")
    {
        await ProactiveInstallationService.HandleInstallCommand(context, config);
        return;
    }

    // Handle send command - send proactive messages to team members
    if (textLower == "send")
    {
        await ProactiveInstallationService.HandleSendCommand(context, config);
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

    // If not signed in, trigger sign-in
    if (!context.IsSignedIn)
    {
        await context.SignIn();
        return;
    }

    // User is signed in - display user details
    try
    {
        var graphClient = context.GetUserGraphClient();
        var me = await graphClient.Me.GetAsync();
        var jobTitle = me?.JobTitle ?? "null";
        
        await context.Send($"You're logged in as {me!.DisplayName} ({me.UserPrincipalName}); your job title is: {jobTitle}; your photo is:");

        try
        {
            var photoStream = await graphClient.Me.Photo.Content.GetAsync();
            if (photoStream != null)
            {
                using var memoryStream = new MemoryStream();
                await photoStream.CopyToAsync(memoryStream);
                var base64Photo = Convert.ToBase64String(memoryStream.ToArray());
                await context.Send($"<img src=\"data:image/jpeg;base64,{base64Photo}\" alt=\"Profile Photo\" />");
            }
        }
        catch { /* Profile photo not available */ }

        if (!string.IsNullOrEmpty(userState.Token))
        {
            await context.Send("Would you like to view your token?\n\n**Yes or No**");
            userState.WaitingForTokenConfirmation = true;
            SetUserState(userId, userState);
        }
        else
        {
            await context.Send("Type 'logout' then 'signin' to get a fresh token.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching user details: {ex.Message}");
    }
});

// Map proactive installation endpoints
ProactiveInstallationService.MapProactiveEndpoints(app, config, conversationReferences);

app.Run();

// Model Classes

public class ConfigOptions { public TeamsConfigOptions? Teams { get; set; } }
public class TeamsConfigOptions { public string? ClientId { get; set; } public string? ClientSecret { get; set; } public string? TenantId { get; set; } public string? ConnectionName { get; set; } public string? TeamsAppId { get; set; } }
public class UserState { public bool WaitingForTokenConfirmation { get; set; } public string? Token { get; set; } }
public class ConversationReferenceData { public string? UserId { get; set; } public string? ConversationId { get; set; } public string? ServiceUrl { get; set; } public string? ChannelId { get; set; } public string? TenantId { get; set; } }
public class ProactiveNotifyRequest { public string? Message { get; set; } }
