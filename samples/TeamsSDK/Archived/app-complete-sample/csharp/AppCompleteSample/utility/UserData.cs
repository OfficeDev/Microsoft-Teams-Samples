using AppCompleteSample.Utility;
using System.Collections.Generic;

namespace AppCompleteSample.utility
{
    /// <summary>
    /// Represents the user data.
    /// </summary>
    public class UserData
    {
        /// <summary>
        /// Gets or sets the compose extension card type.
        /// </summary>
        public string ComposeExtensionCardType { get; set; }

        /// <summary>
        /// Gets or sets the bot ID.
        /// </summary>
        public string BotId { get; set; }

        /// <summary>
        /// Gets or sets the channel ID.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the conversation ID.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the service URL.
        /// </summary>
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the compose extension selected results.
        /// </summary>
        public List<WikiHelperSearchResult> ComposeExtensionSelectedResults { get; set; }
    }
}