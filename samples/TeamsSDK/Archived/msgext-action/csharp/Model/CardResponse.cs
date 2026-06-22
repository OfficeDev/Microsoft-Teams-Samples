namespace Microsoft.BotBuilderSamples.Models
{
    /// <summary>
    /// Represents the response data for a card.
    /// </summary>
    public class CardResponse
    {
        /// <summary>
        /// Gets or sets the title of the card.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the subtitle of the card.
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets the text of the card.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the username for the card.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the user password for the card.
        /// </summary>
        public string UserPwd { get; set; }
    }
}