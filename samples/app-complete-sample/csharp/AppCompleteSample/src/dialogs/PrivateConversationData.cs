using Microsoft.Bot.Schema;

namespace AppCompleteSample.src.dialogs
{
    /// <summary>
    /// Represents the private conversation data.
    /// </summary>
    public class PrivateConversationData
    {
        /// <summary>
        /// Gets or sets the authentication token key.
        /// </summary>
        public string AuthTokenKey { get; set; }

        /// <summary>
        /// Gets or sets the persisted cookie.
        /// </summary>
        public ConversationReference PersistedCookie { get; set; }

        /// <summary>
        /// Gets or sets the persisted cookie for VSTS.
        /// </summary>
        public ConversationReference PersistedCookieVSTS { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the VSTS authentication token key.
        /// </summary>
        public string VSTSAuthTokenKey { get; set; }
    }

    /// <summary>
    /// Represents the user conversation state.
    /// </summary>
    public class UserConversationState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the welcome card is sent.
        /// </summary>
        public bool IsWelcomeCardSent { get; set; }
    }
}