// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedChannelEvents;
using SharedChannelEvents.Bots;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson();

// Create the Bot Framework Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, SharedChannelDataBot>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Buffer the request body so the Bot adapter can read it even if the client resets the stream.
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();