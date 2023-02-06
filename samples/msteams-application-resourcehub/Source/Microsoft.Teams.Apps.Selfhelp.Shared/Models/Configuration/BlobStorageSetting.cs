namespace Microsoft.Teams.Apps.Selfhelp.Shared.Models.Configuration
{
    /// <summary>
    /// A class which helps to provide Blob storage settings.
    /// </summary>
    public class BlobStorageSetting : BotSettings
    {
        /// <summary>
        /// Gets or sets storage connection string.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}