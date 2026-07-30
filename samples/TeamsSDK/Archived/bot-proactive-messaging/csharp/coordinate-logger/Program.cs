// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace msteams_app_coordinatelogger
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.CoordinateLogger.Bot;
    using Microsoft.Teams.CoordinateLogger.Services;

    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Preserve sample-specific local override behavior.
            builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

            string appId = builder.Configuration.GetValue<string>("App:Id");
            string appPassword = builder.Configuration.GetValue<string>("App:Password");

            builder.Services
                .AddSingleton(new MicrosoftAppCredentials(appId, appPassword));

            builder.Services
                .AddSingleton<IConnectorClientFactory, ConnectorClientFactory>();

            builder.Services
                .AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            builder.Services
                .AddTransient<CloudAdapter>(sp =>
                    new CloudAdapter(
                        sp.GetRequiredService<BotFrameworkAuthentication>(),
                        sp.GetRequiredService<ILogger<CloudAdapter>>()));

            builder.Services
                .AddTransient<IBot, CoordinateLoggerActivityHandler>();

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseRequestLocalization();
            app.MapControllers();

            app.Run();
        }
    }
}
