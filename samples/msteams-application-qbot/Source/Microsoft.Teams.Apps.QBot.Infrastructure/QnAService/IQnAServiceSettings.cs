namespace Microsoft.Teams.Apps.QBot.Infrastructure.QnAService
{
    /// <summary>
    /// QnAService settings contract.
    /// </summary>
    internal interface IQnAServiceSettings
    {
        /// <summary>
        /// Gets knowledge base id.
        /// </summary>
        string KnowledgeBaseId { get; }

        /// <summary>
        /// Gets score threshold.
        /// </summary>
        int ScoreThreshold { get; }
    }
}
