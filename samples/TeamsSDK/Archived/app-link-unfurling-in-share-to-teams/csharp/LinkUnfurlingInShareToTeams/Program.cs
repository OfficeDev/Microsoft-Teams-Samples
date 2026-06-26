// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using LinkUnfurlingInShareToTeams;
using LinkUnfurlingInShareToTeams.Bots;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

// Create the Bot Framework Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the storage we'll be using for User and Conversation state.
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Create the Conversation state.
builder.Services.AddSingleton<ConversationState>();

// Create the bot as a transient.
builder.Services.AddTransient<IBot, ActivityBot>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles()
   .UseStaticFiles()
   .UseRouting()
   .UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

