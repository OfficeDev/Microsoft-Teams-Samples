using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CallingBotSample.Common
{
    public class Constants
    {

        /// <summary>
        /// Azure Client Id.
        /// </summary>
        public const string ClientIdConfigurationSettingsKey = "AzureAd:ClientId";

        /// <summary>
        /// Azure Tenant Id.
        /// </summary>
        public const string TenantIdConfigurationSettingsKey = "AzureAd:TenantId";

        /// <summary>
        /// Azure ClientSecret .
        /// </summary>
        public const string ClientSecretConfigurationSettingsKey = "AzureAd:ClientSecret";

        public const string UserIdConfigurationSettingsKey = "UserId";

        public const string MicrosoftAppPasswordConfigurationSettingsKey = "MicrosoftAppPassword";

        public const string MicrosoftAppIdConfigurationSettingsKey = "MicrosoftAppId";

        public const string BotBaseUrlConfigurationSettingsKey = "BotBaseUrl";
    }
}
