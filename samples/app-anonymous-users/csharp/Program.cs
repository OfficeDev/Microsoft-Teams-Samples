// <copyright file="Program.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.BotBuilderSamples;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using System;
using AnonymousUsers;
using AnonymousUsers.Bots;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
});

// In production, the React files will be served from this directory
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/build";
});

// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Bot Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, AnonymousUsersBot>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.l
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles()
    .UseStaticFiles()
    .UseRouting()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapHub<ChatHub>("/chatHub");
    });

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";

    if (app.Environment.IsDevelopment())
    {
        spa.Options.StartupTimeout = TimeSpan.FromSeconds(120);
        spa.UseReactDevelopmentServer(npmScript: "start");
    }
});

app.Run();