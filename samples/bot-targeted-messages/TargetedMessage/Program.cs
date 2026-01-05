using TargetedMessage;
using TargetedMessage.Controllers;
using Microsoft.Teams.Api.Auth;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Common.Http;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.Get<ConfigOptions>();
var appBuilder = App.Builder();

builder.Services.AddSingleton<ReminderController>();
builder.Services.AddSingleton<IHttpClient>(sp =>
{
    var teamsApp = sp.GetRequiredService<App>();
    return teamsApp.Client;
});

builder.AddTeams(appBuilder);

var app = builder.Build();
app.UseTeams();
app.Run();