// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using SuggestedActions;
using SuggestedActions.Controllers;
using Azure.Core;
using Azure.Identity;
using Microsoft.Teams.Api.Auth;
using Microsoft.Teams.Apps;
using Microsoft.Teams.Apps.Extensions;
using Microsoft.Teams.Common.Http;
using Microsoft.Teams.Plugins.AspNetCore.Extensions;

// Create web application builder and load configuration
var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration.Get<ConfigOptions>();

// Create Teams app builder
var appBuilder = App.Builder();

// Register controller and configure Teams services
builder.Services.AddSingleton<Controller>();
builder.AddTeams(appBuilder);

// Build and run the application
var app = builder.Build();
app.UseTeams();
app.Run();