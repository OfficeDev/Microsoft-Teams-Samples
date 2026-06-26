// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.BotBuilderSamples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add necessary services to the container.
ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
ConfigureApp(app);

app.Run();

void ConfigureServices(IServiceCollection services)
{
    // Add HTTP client and JSON configuration.
    services.AddHttpClient().AddControllers().AddNewtonsoftJson();

    // Register the Bot Framework Adapter with error handling.
    services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

    // Register Bot Framework Authentication.
    services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

    // Register state management services. Consider using Scoped for better isolation.
    services.AddScoped<IStorage, MemoryStorage>(); // Consider replacing MemoryStorage with persistent storage for production.
    services.AddScoped<UserState>();
    services.AddScoped<ConversationState>();

    // Register the dialog to be used by the bot.
    services.AddSingleton<MainDialog>();

    // Register the bot as a transient service.
    services.AddTransient<IBot, TeamsBot>();
}

void ConfigureApp(WebApplication app)
{
    // Environment-specific configurations.
    if (!app.Environment.IsDevelopment())
    {
        // Enabling HTTPS Strict Transport Security (HSTS) in production environments.
        app.UseHsts();
    }
    else
    {
        // Enabling developer exception page during development.
        app.UseDeveloperExceptionPage();
    }

    // Serving static files and configuring routing.
    app.UseDefaultFiles()
       .UseStaticFiles()
       .UseRouting()
       .UseAuthorization();

    // Configure endpoints to handle incoming requests.
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers(); // Mapping controller routes.
    });
}
