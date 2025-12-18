using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Activities;
using Microsoft.Teams.Apps.Events;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Extensions.Graph;
using Microsoft.Teams.Common.Logging;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;
using TeamsAuth;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.Get<ConfigOptions>();


var appBuilder = App.Builder()
    .AddOAuth(config.Teams.ConnectionName);

builder.AddTeams(appBuilder);

var app = builder.Build();
var teams =app.UseTeams();
var token = "";


teams.Use(async context =>
{
    var start = DateTime.UtcNow;
    try
    {
        await context.Next();
    }
    catch
    {
        context.Log.Error("error occurred during activity processing");
    }
    context.Log.Debug($"request took {(DateTime.UtcNow - start).TotalMilliseconds}ms");
});

teams.OnMembersAdded(async context =>
{
    await context.Send("Welcome to AuthenticationBot. Type '/signin' to get logged in. Type '/signout' to sign out.");
});



teams.OnMessage("/signout", async context =>
{
    if (!context.IsSignedIn)
    {
        await context.Send("you are not signed in!");
        return;
    }

    await context.SignOut();
    await context.Send("You have been signed out!");
});

teams.OnMessage("/signin", async context =>
{
    if (!context.IsSignedIn)
    {
        await context.SignIn(); 

        return;
    }

    var me = await context.GetUserGraphClient().Me.GetAsync();
    await context.Send($"User '{me!.DisplayName}' is already signed in!");
});

teams.OnSignIn(async (_, @event) =>
{
   token = @event.Token.Token;
    var context = @event.Context;

    var me = await context.GetUserGraphClient().Me.GetAsync();
    await context.Send($"You have logged in as \"{me!.DisplayName}\". Would you like to view your token, type 'Yes' to view else type 'No' to cancel:");
});

teams.OnMessage(async context =>
{
    var command = context.Activity.Text.Trim().ToLowerInvariant();
    if (command.Equals("yes"))
    {
        await context.Send(token);
        await context.Send("Thank You!");
        return;
    }
    else
    {
        await context.Send("Thank You!");
        return;
    }
});

app.Run();