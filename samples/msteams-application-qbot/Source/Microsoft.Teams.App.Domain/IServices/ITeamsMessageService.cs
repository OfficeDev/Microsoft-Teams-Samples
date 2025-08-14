namespace Microsoft.Teams.Apps.QBot.Domain.IServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.QBot.Domain.Models;

    /// <summary>
    /// Teams Messaging service contract.
    /// </summary>
    public interface ITeamsMessageService
    {
        /// <summary>
        /// Gets question message.
        /// </summary>
        /// <param name="teamId">Team Id (AAD object id).</param>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="questionMessageId">Question Message id.</param>
        /// <returns>Question message.</returns>
        Task<string> GetQuestionMessageAsync(string teamId, string channelId, string questionMessageId);

        /// <summary>
        /// Gets question message for list of questions.
        /// </summary>
        /// <param name="teamId">Team Id (AAD object id).</param>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="questionsMessageIds">List of question message id.</param>
        /// <returns>Dictionary of Question message id and message description.</returns>
        Task<IDictionary<string, string>> GetQuestionsMessageAsync(string teamId, string channelId, IEnumerable<string> questionsMessageIds);

        /// <summary>
        /// Returns a list of responses to a question.
        /// </summary>
        /// <param name="teamId">Team Id (AAD object id).</param>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="questionMessageId">Question Message id.</param>
        /// <returns>A list of responses to a question.</returns>
        Task<IEnumerable<QuestionResponse>> GetQuestionResponsesAsync(string teamId, string channelId, string questionMessageId);

        /// <summary>
        /// Returns a response to question.
        /// </summary>
        /// <param name="teamId">Team Id (AAD object id).</param>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="questionMessageId">Question Message id.</param>
        /// <param name="responseMessageId">Response Message id.</param>
        /// <returns>A response to a question.</returns>
        Task<QuestionResponse> GetQuestionResponseAsync(string teamId, string channelId, string questionMessageId, string responseMessageId);

        /// <summary>
        /// Posts correct answer.
        /// </summary>
        /// <param name="question">Question.</param>
        /// <param name="answer">Answer.</param>
        /// <param name="users">Users profiles.</param>
        /// <returns>Message id for the posted message.</returns>
        Task<string> PostCorrectAnswerAsync(Question question, Answer answer, IEnumerable<User> users);

        /// <summary>
        /// Post select answer response.
        /// </summary>
        /// <param name="question">Question.</param>
        /// <param name="users">Users profiles.</param>
        /// <returns>Message id for the posted message.</returns>
        Task<string> PostSelectAnswerAsync(Question question, IEnumerable<User> users);

        /// <summary>
        /// Post "suggest answer" response.
        /// </summary>
        /// <param name="question">Question.</param>
        /// <param name="suggestedAnswer">Suggested Answer.</param>
        /// <returns>Message id for the posted message.</returns>
        Task<string> PostSuggestAnswerAsync(Question question, SuggestedAnswer suggestedAnswer);

        /// <summary>
        /// Updates Bot suggested message when it is deemed not helpful.
        /// </summary>
        /// <param name="question">Question.</param>
        /// <param name="answer">answer.</param>
        /// <param name="user">User who marked it as not helpful.</param>
        /// <returns>Task.</returns>
        Task<string> UpdateSuggestedAnswerAsync(Question question, Answer answer, User user);

        /// <summary>
        /// Notify question author that the question is answered.
        /// </summary>
        /// <param name="course">Course.</param>
        /// <param name="channel">Channel.</param>
        /// <param name="question">Question object.</param>
        /// <param name="answer">Answer object.</param>
        /// <returns>Async Task.</returns>
        Task NotifyQuestionAnsweredAsync(Course course, Channel channel, Question question, Answer answer);

        /// <summary>
        /// Notify answer author that his response was selected as answer.
        /// </summary>
        /// <param name="course">Course.</param>
        /// <param name="channel">Channel.</param>
        /// <param name="question">Question object.</param>
        /// <param name="answer">Answer object.</param>
        /// <returns>Async Task.</returns>
        Task NotifyAnsweredAcceptedAsync(Course course, Channel channel, Question question, Answer answer);

        /// <summary>
        /// Notify a member to set-up course.
        /// </summary>
        /// <param name="course">Course.</param>
        /// <param name="member">Member.</param>
        /// <returns>Async task.</returns>
        Task NotifyMemberToSetupCourseAsync(Course course, Member member);

        /// <summary>
        /// Updates select answer message by removing select answer button and a link to answer message.
        /// </summary>
        /// <param name="course">Course.</param>
        /// <param name="channel">Channel.</param>
        /// <param name="question">Question.</param>
        /// <param name="answer">Answer.</param>
        /// <returns>Message id</returns>
        Task<string> UpdateSelectAnswerMessageAsync(Course course, Channel channel, Question question, Answer answer);
    }
}
