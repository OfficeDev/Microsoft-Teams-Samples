using Microsoft.Bot.Schema;

namespace Microsoft.Teams.TemplateBotCSharp.src.dialogs
{
    public class PrivateConversationData
    {
        public string AuthTokenKey { get; set; }
        public ConversationReference PersistedCookie { get; set; }

        public ConversationReference PersistedCookieVSTS { get; set; }

        public string Name { get; set; }
        public string VSTSAuthTokenKey { get; set; }
    }
    public class UserConversationState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the welcome card is sent to user or not.
        /// </summary>
        /// <remark>Value is null when bot is installed for first time.</remark>
        public bool IsWelcomeCardSent { get; set; }
    }
}