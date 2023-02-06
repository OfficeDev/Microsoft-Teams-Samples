namespace Microsoft.Teams.Apps.QBot.Domain.Models
{
    /// <summary>
    /// Suggested answer from QnA Service.
    /// </summary>
    public sealed class SuggestedAnswer
    {
        /// <summary>
        /// Gets or sets QnA Pair Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets answer.
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets confidence score.
        /// </summary>
        public int Score { get; set; }
    }
}
