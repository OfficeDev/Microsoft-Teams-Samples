using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Microsoft.BotBuilderSamples.SPListBot.Repositories
{
    public class SettingsConfig
    {
        private static IConfiguration? s_configuration;

        /// <summary>
        /// Gets an application setting by key from appsettings.json.
        /// </summary>
        /// <param name="key">The configuration key to retrieve.</param>
        /// <returns>The configuration value.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the setting is not found.</exception>
        public static string AppSetting(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var configuration = GetConfiguration();
            var value = configuration.GetValue<string>(key);

            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Configuration key '{key}' not found or is empty.");
            }

            return value;
        }

        /// <summary>
        /// Gets or initializes the configuration.
        /// </summary>
        private static IConfiguration GetConfiguration()
        {
            if (s_configuration != null)
            {
                return s_configuration;
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            s_configuration = builder.Build();
            return s_configuration;
        }
    }
}