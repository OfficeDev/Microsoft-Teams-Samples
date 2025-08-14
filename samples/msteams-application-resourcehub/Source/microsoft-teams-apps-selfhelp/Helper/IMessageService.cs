namespace Microsoft.Teams.Apps.Selfhelp.Helper
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Selfhelp.Shared.Models.Entity;

    /// <summary>
    /// Teams message service.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Send message to a conversation.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="conversationId">Conversation Id.</param>
        /// <param name="serviceUrl">The service URL to use for sending the notification.</param>
        /// <param name="maxAttempts">Max attempts to send the message.</param>
        /// <param name="logger">Logger.</param>
        /// <returns>Send message response.</returns>
        public Task<SendMessageResponse> SendMessageAsync(
            IMessageActivity message,
            string conversationId,
            string serviceUrl,
            int maxAttempts,
            ILogger logger);

        /// <summary>
        /// Get the user adaptive card.
        /// </summary>
        /// <param name="cardMessage">Message on card.</param>
        /// <param name="learningId">Id of learning content.</param>
        /// <param name="environmentCurrentDirectory">Environment current directory details.</param>
        /// <param name="sharedByUser">Content shared by user.</param>
        /// <returns>Returns the user adaptive card.</returns>
        Task<Bot.Schema.Attachment> GetUserAdaptiveCard(string cardMessage, string learningId, string environmentCurrentDirectory, string sharedByUser);

        /// <summary>
        /// Send notification card to the user.
        /// </summary>
        /// <param name="title">Title of learning content.</param>
        /// <param name="articleCheckBoxEntities">Article checkbox entities details.</param>
        /// <param name="environmentCurrentDirectory">Environment current directory details.</param>
        /// <param name="clientId">Unique id of client.</param>
        /// <returns>Returns send notification card to the user.</returns>
        Task<Attachment> SendNotificationtoUsersCard(string title, List<ArticleCheckBoxEntity> articleCheckBoxEntities, string environmentCurrentDirectory, string clientId);

        /// <summary>
        /// Send welcome card to the user.
        /// </summary>
        /// <param name="environmentCurrentDirectory">Environment current directory details.</param>
        /// <param name="clientId">Unique id of client.</param>
        /// <returns>Returns send welcome card to the user.</returns>
        Task<Attachment> SendWelcomeCardToUser(string environmentCurrentDirectory, string clientId);
    }
}