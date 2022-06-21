// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MeetingLiveCaption.Models.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Adds application configuration settings to specified IServiceCollection.
builder.Services.AddOptions<MeetingSettings>()
.Configure<IConfiguration>((botOptions, configuration) =>
{
    botOptions.CART_URL = configuration.GetValue<string>("CART_URL");
});

builder.Services.AddHttpClient().AddControllers().AddNewtonsoftJson();

builder.Services.AddMvc().AddSessionStateTempDataProvider();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles()
    .UseStaticFiles()
    .UseWebSockets()
    .UseRouting()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    });

app.Run();