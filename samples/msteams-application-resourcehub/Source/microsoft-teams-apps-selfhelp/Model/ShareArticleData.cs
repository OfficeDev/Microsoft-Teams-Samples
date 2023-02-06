namespace Microsoft.Teams.Selfhelp.Authentication.Model
{
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Enum;

    /// <summary>
    /// Shared article data model class.
    /// </summary>
    public class ShareArticleData
    {
        /// <summary>
        /// Gets or sets item type.
        /// </summary>
        public ItemType ItemType { get; set; }

        /// <summary>
        /// Gets or sets learning id.
        /// </summary>
        public string LearningId { get; set; }
    }
}