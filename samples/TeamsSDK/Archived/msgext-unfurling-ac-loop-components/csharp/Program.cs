// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Agents.Hosting.AspNetCore;
using MsgextUnfurlingAcLoop.Bots;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

// Add AgentApplicationOptions from appsettings section "AgentApplication".
builder.AddAgentApplicationOptions();

// Add the Agent, which contains the logic for responding to user messages.
builder.AddAgent<MsgextUnfurlingAcLoopComponents>();

// Configure the HTTP request pipeline.

// Add AspNet token validation for Azure Bot Service and Entra.
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles()
    .UseStaticFiles();

app.MapGet("/", () => "Microsoft Agents SDK Sample");

// This receives incoming messages and routes them to the registered AgentApplication.
app.MapAgentApplicationEndpoints(requireAuth: !app.Environment.IsDevelopment());

app.Run();