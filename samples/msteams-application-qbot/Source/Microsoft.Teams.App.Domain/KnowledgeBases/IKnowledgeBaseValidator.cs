namespace Microsoft.Teams.Apps.QBot.Domain.KnowledgeBases
{
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// KnowledgeBase validator.
    /// </summary>
    public interface IKnowledgeBaseValidator
    {
        /// <summary>
        /// Validates if KnowledgeBase's properties are valid.
        /// </summary>
        /// <param name="knowledgeBase">KB object.</param>
        /// <returns>true if the kb properties are valid, false otherwise.</returns>
        bool IsValid(KnowledgeBase knowledgeBase);
    }
}
