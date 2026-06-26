// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Catering;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services
    .AddControllers()
    .AddNewtonsoftJson();

// Create the Bot Framework Adapter with error handling enabled
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Add bot recognizer and database services
builder.Services.AddSingleton<CateringRecognizer>();
builder.Services.AddTransient<CateringDb>();

// Add the bot as a transient service
builder.Services.AddTransient<IBot, CateringBot>();

// Add storage and state services
builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddSingleton<UserState>();

// Configure logging
builder.Logging.AddDebug();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles()
   .UseStaticFiles()
   .UseRouting()
   .UseAuthorization();

app.MapControllers();

app.Run();
