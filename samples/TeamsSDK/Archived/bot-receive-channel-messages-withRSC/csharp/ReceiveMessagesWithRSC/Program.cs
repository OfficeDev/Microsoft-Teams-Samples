// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using ReceiveMessagesWithRSC;
using ReceiveMessagesWithRSC.Bots;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient()
    .AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(sp => sp.GetRequiredService<CloudAdapter>());
builder.Services.AddTransient<IBot, ActivityBot>();

var app = builder.Build();

app.UseDefaultFiles()
    .UseStaticFiles()
    .UseWebSockets()
    .UseRouting()
    .UseAuthorization();

app.MapControllers();

app.Run();