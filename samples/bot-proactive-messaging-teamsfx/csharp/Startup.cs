using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.BotBuilderSamples.Bots;
//using YourNamespace.Bots; // Replace with actual namespace
using Microsoft.Bot.Connector.Authentication;
using Microsoft.BotBuilderSamples;
using Microsoft.Bot.Schema;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add controller support and JSON settings
        services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
        });

        // Add Bot authentication (appId + appPassword from appsettings.json or environment)
        services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

        // Add custom adapter with error handling
        services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

        // Add a thread-safe in-memory store for conversation references (for proactive messaging)
        services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

        // Register your bot class
        services.AddTransient<IBot, TeamsBot>(); // Replace 'TeamsBot' with your class name
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseDefaultFiles()
           .UseStaticFiles()
           .UseRouting()
           .UseAuthorization()
           .UseEndpoints(endpoints =>
           {
               endpoints.MapControllers(); // Enables routing for /api/messages and /api/notify
           });
    }
}