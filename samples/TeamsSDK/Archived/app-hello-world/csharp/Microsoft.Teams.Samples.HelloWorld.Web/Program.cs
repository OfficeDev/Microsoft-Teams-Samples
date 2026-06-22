// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Teams.Samples.HelloWorld.Web;

var builder = WebApplication.CreateBuilder(args);

// Add controllers and MVC services
builder.Services.AddControllers();
builder.Services.AddMvc();

// Bot Framework Authentication
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Bot Framework HTTP Adapter with custom error handler
builder.Services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

// Register the bot as a transient service
builder.Services.AddTransient<IBot, MessageExtension>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();
app.UseRouting();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
