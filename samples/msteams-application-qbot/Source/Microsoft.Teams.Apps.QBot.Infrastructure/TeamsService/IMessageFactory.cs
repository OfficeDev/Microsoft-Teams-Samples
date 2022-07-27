namespace Microsoft.Teams.Apps.QBot.Infrastructure.TeamsService
{
    using System.Collections.Generic;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Message factory inteface.
    /// </summary>
    public interface IMessageFactory
    {
        /// <summary>
        /// Creates "select answer" message and tags users.
        /// </summary>
        /// <param name="question">Question object.</param>
        /// <param name="users">User profiles for users to be tagged.</param>
        /// <returns>Attachment.</returns>
        Attachment CreateSelectAnswerMessage(Question question, IEnumerable<User> users);

        /// <summary>
        /// Creates an answer message.
        /// </summary>
        /// <param name="question">Question object.</param>
        /// <param name="answer">Answer object.</param>
        /// <param name="users">User profiles for user ids in Answer/Question object.</param>
        /// <returns>Attachment.</returns>
        Attachment CreateAnswerMessage(Question question, Answer answer, IEnumerable<User> users);

        /// <summary>
        /// Creates "suggest answer" message.
        /// </summary>
        /// <param name="question">Question object.</param>
        /// <param name="suggestedAnswer">Suggested Answer.</param>
        /// <returns>Attachment.</returns>
        Attachment CreateSuggestAnswerMessage(Question question, SuggestedAnswer suggestedAnswer);

        /// <summary>
        /// Creates a message when user marks suggested answer as not helpful.
        /// </summary>
        /// <param name="answer">Answer object.</param>
        /// <param name="user">User.</param>
        /// <returns>Attachment.</returns>
        Attachment CreateNotHelpfulMessage(Answer answer, User user);

        /// <summary>
        /// Creates an error message. (adaptive card).
        /// </summary>
        /// <param name="errorMessage">Error message.</param>
        /// <returns>Attachment.</returns>
        Attachment CreateErrorMessage(string errorMessage);

        /// <summary>
        /// Creates a question answered message with deeplink to answer message.
        /// </summary>
        /// <param name="deepLink">Deeplink to answer message.</param>
        /// <returns>Attachment.</returns>
        Attachment CreateQuestionAnsweredMessage(string deepLink);

        /// <summary>
        /// Creates a message that explains how to post a new question.
        /// </summary>
        /// <returns>Attachment.</returns>
        Attachment CreateNewThreadMessage();
    }
}
