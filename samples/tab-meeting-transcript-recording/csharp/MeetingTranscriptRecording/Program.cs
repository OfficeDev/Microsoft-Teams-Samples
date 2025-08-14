// <copyright file="Program.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

using MeetingTranscriptRecording;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient().AddControllers();

// In production, the React files will be served from this directory
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/build";
});


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