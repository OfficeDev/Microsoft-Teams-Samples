namespace Microsoft.Teams.Apps.QBot.Infrastructure.QnAService
{
    /// <summary>
    /// QnA Service settings.
    /// </summary>
    internal class QnAServiceSettings : IQnAServiceSettings
    {
        /// <summary>
        /// Gets or sets knowledge base id.
        /// </summary>
        public string KnowledgeBaseId { get; set; }

        /// <summary>
        /// Gets or sets score threshold.
        /// </summary>
        public int ScoreThreshold { get; set; }
    }
}
