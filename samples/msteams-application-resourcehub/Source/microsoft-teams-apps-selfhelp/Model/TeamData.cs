namespace Microsoft.Teams.Selfhelp.Authentication.Model
{
    /// <summary>
    /// Team data model class.
    /// </summary>
    public class TeamData
    {
        /// <summary>
        /// Gets or sets team id.
        /// </summary>
        public string teamId { get; set; }

        /// <summary>
        /// Gets or sets team name.
        /// </summary>
        public string teamName { get; set; }

        /// <summary>
        /// Gets or sets channel id.
        /// </summary>
        public string channelId { get; set; }

        /// <summary>
        /// Gets or sets channel name.
        /// </summary>
        public string channelName { get; set; }
    }
}