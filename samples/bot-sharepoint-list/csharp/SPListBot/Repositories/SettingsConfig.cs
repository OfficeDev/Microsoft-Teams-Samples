using Microsoft.Extensions.Configuration;
using System.IO;

namespace Microsoft.BotBuilderSamples.SPListBot.Repositories
{
    public class SettingsConfig
    {
        private static SettingsConfig _appSettings;
        public string AppSettingValue { get; set; }
        
        public static string AppSetting(string Key)
        {
            _appSettings = GetCurrentSettings(Key);
            return _appSettings.AppSettingValue;
        }

        public SettingsConfig(IConfiguration config, string Key)
        {
            this.AppSettingValue = config.GetValue<string>(Key);
        }

        // Get a valued stored in the appsettings.
        // Pass in a key like TestArea:TestKey to get TestValue
        public static SettingsConfig GetCurrentSettings(string Key)
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();

            var settings = new SettingsConfig(configuration, Key);

            return settings;
        }
    }
}